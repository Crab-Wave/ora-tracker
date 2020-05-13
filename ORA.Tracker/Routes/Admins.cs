using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Admins : Route
    {
        private static readonly string missingClusterId = new Error("Missing cluster id").ToString();
        private static readonly string invalidToken = new Error("Invalid token").ToString();
        private static readonly string invalidClusterId = new Error("Invalid Cluster id").ToString();
        private static readonly string unauthorizedAction = new Error("Unauthorized action").ToString();
        private static readonly string missingMemberId = new Error("Missing member id").ToString();
        private static readonly string notClusterMember = new Error("id does not correspond to a cluster member").ToString();

        private static readonly string notClusterAdmin = new Error("id does not correspond to a cluster admin").ToString();

        public Admins(IServiceCollection services)
            : base(services) { }

        protected override byte[] get(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (!c.members.ContainsKey(this.services.TokenManager.GetIdFromToken(token)))
                throw new HttpListenerException(403, unauthorizedAction);

            return c.SerializeAdmins();
        }

        protected override byte[] post(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string id = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            this.services.TokenManager.RefreshToken(token);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (this.services.TokenManager.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(403, unauthorizedAction);

            if (!c.members.ContainsKey(id))     // return error if is not member ?
                throw new HttpListenerException(400, notClusterMember);

            c.admins.Add(id);
            this.services.ClusterManager.Put(request.UrlParameters["id"], c);

            return new byte[] { };
        }

        protected override byte[] delete(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string adminId = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            this.services.TokenManager.RefreshToken(token);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (this.services.TokenManager.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(403, unauthorizedAction);

            if (!c.admins.Contains(adminId))
                throw new HttpListenerException(400, notClusterAdmin);

            c.admins.Remove(adminId);
            this.services.ClusterManager.Put(request.UrlParameters["id"], c);

            return new byte[] { };
        }
    }
}
