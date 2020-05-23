using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Join : Route
    {
        private static readonly byte[] invalidClusterId = new Error("Invalid Cluster id").ToBytes();
        private static readonly byte[] notAllowedJoinCluster = new Error("Not allowed to join this cluster").ToBytes();

        public Join(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        [RequiredQueryParameters("id", "name")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string clusterId = request.QueryString["id"];
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(clusterId);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            string id = this.services.TokenManager.GetIdFromToken(token);
            if (!cluster.HasInvitedIdentity(id))
            {
                response.Forbidden(notAllowedJoinCluster);
                return;
            }

            cluster.invitedIdentities.Remove(id);
            cluster.members[id] = request.QueryString["name"];

            this.services.ClusterManager.Put(cluster);
            response.Close();
        }
    }
}
