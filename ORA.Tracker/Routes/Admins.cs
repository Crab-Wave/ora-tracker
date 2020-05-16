using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;
using ORA.Tracker.Routes.Attributes;

namespace ORA.Tracker.Routes
{
    public class Admins : Route
    {
        private static readonly byte[] missingClusterId = new Error("Missing cluster id").ToBytes();
        private static readonly byte[] invalidClusterId = new Error("Invalid Cluster id").ToBytes();
        private static readonly byte[] unauthorizedAction = new Error("Unauthorized action").ToBytes();
        private static readonly byte[] notClusterMember = new Error("id does not correspond to a cluster member").ToBytes();

        private static readonly byte[] notClusterAdmin = new Error("id does not correspond to a cluster admin").ToBytes();

        public Admins(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;

            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
            {
                response.BadRequest(missingClusterId);
                return;
            }

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (c == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!c.members.ContainsKey(this.services.TokenManager.GetIdFromToken(token)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            response.Close(c.SerializeAdmins(), true);
        }

        [Authenticate]
        [RequiredQueryParameters("id")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string id = request.QueryString["id"];

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

            if (!c.members.ContainsKey(id))     // return error if is not member ?
            {
                response.BadRequest(notClusterMember);
                return;
            }

            c.admins.Add(id);
            this.services.ClusterManager.Put(request.UrlParameters["id"], c);

            response.Close();
        }

        [Authenticate]
        [RequiredQueryParameters("id")]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string adminId = request.QueryString["id"];

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

            if (!c.admins.Contains(adminId))
            {
                response.BadRequest(notClusterAdmin);
                return;
            }

            c.admins.Remove(adminId);
            this.services.ClusterManager.Put(request.UrlParameters["id"], c);

            response.Close();
        }
    }
}
