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

            if (!cluster.HasMember(this.services.TokenManager.GetIdFromIp(request.Ip)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            response.Close(cluster.SerializeMembers(), true);
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
        [RequiredQueryParameters("id")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
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

            string userId = this.services.TokenManager.GetIdFromIp(request.Ip);
            if (!cluster.IsOwnedBy(userId) && !cluster.HasAdmin(userId))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            if (!cluster.HasInvitedIdentity(id) && !cluster.HasMember(id))
            {
                cluster.invitedIdentities.Add(id);
                this.services.ClusterManager.Put(cluster);
            }

            response.Close();
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
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

            string userId = this.services.TokenManager.GetIdFromIp(request.Ip);
            if (!cluster.IsOwnedBy(userId) && !cluster.HasAdmin(userId))
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
