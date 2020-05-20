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

            response.Close(cluster.SerializeWithoutMemberName(), true);
        }

        [Authenticate]
        [RequiredQueryParameters("name", "username")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            this.services.TokenManager.RefreshToken(token);

            var cluster = new Cluster(request.QueryString["name"],
                this.services.TokenManager.GetIdFromToken(token), request.QueryString["username"]);
            this.services.ClusterManager.Put(cluster.id.ToString(), cluster);

            response.Close(cluster.SerializeId(), true);
        }

        [Authenticate]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;

            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
            {
                response.BadRequest(missingClusterId);
                return;
            }

            this.services.TokenManager.RefreshToken(token);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (c == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (this.services.TokenManager.GetIdFromToken(token) != c.owner)
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            this.services.ClusterManager.Delete(request.UrlParameters["id"]);
            response.Close();
        }
    }
}
