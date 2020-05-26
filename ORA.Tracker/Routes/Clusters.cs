using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        private static readonly byte[] invalidClusterId = new Error("Invalid Cluster id").ToBytes();
        private static readonly byte[] missingClusterId = new Error("Missing Cluster id").ToBytes();
        private static readonly byte[] unauthorizedAction = new Error("Unauthorized action").ToBytes();

        public Clusters(IServiceCollection services)
            : base(services) { }

        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
            {
                response.Close(this.services.ClusterManager.GetAll(), true);
                return;
            }

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            response.Close(cluster.SerializePublicInformation(), true);
        }

        [Authenticate]
        [RequiredQueryParameters("name", "username")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            this.services.TokenManager.RefreshToken(token);

            var cluster = new Cluster(request.QueryString["name"],
                this.services.TokenManager.GetIdFromIp(request.Ip), request.QueryString["username"]);
            this.services.ClusterManager.Put(cluster);

            response.Close(cluster.SerializeId(), true);
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;

            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.IsOwnedBy(this.services.TokenManager.GetIdFromIp(request.Ip)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            this.services.ClusterManager.Delete(request.UrlParameters["id"]);
            response.Close();
        }
    }
}
