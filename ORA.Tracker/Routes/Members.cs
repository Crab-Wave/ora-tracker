using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class Members : Route
    {
        private static readonly byte[] missingClusterId = new Error("Missing cluster id").ToBytes();
        private static readonly byte[] invalidClusterId = new Error("Invalid Cluster id").ToBytes();
        private static readonly byte[] unauthorizedAction = new Error("Unauthorized action").ToBytes();

        public Members(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;

            // TODO: Check requested URL parameter
            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
            {
                response.BadRequest(missingClusterId);
                return;
            }

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.members.ContainsKey(this.services.TokenManager.GetIdFromToken(token)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            response.Close(cluster.SerializeMembers(), true);
        }

        [Authenticate]
        [RequiredQueryParameters("id", "name")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string id = request.QueryString["id"];
            string name = request.QueryString["name"];

            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            string userId = this.services.TokenManager.GetIdFromToken(token);
            if (userId != cluster.owner && !cluster.admins.Contains(userId))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            cluster.members.Add(id, name);
            this.services.ClusterManager.Put(cluster);

            response.Close();
        }

        [Authenticate]
        [RequiredQueryParameters("id")]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string id = request.QueryString["id"];

            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            string userId = this.services.TokenManager.GetIdFromToken(token);
            if (userId != cluster.owner && !cluster.admins.Contains(userId))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            cluster.members.Remove(id);
            this.services.ClusterManager.Put(cluster);

            response.Close();
        }
    }
}
