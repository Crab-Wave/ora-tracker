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
        [RequiredUrlParameters("id")]
        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.HasMember(this.services.TokenManager.GetIdFromToken(token)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            response.Close(cluster.SerializeAdmins(), true);
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
        [RequiredQueryParameters("id")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string memberId = request.QueryString["id"];
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (this.services.TokenManager.GetIdFromToken(token) != cluster.owner)
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            if (!cluster.HasMember(memberId))
            {
                response.BadRequest(notClusterMember);
                return;
            }

            cluster.admins.Add(memberId);
            this.services.ClusterManager.Put(cluster);

            response.Close();
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
        [RequiredQueryParameters("id")]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string adminId = request.QueryString["id"];
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.IsOwnedBy(this.services.TokenManager.GetIdFromToken(token)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            if (!cluster.HasAdmin(adminId))
            {
                response.BadRequest(notClusterAdmin);
                return;
            }

            cluster.admins.Remove(adminId);
            this.services.ClusterManager.Put(cluster);

            response.Close();
        }
    }
}
