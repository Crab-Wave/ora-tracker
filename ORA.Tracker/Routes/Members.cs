using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Members : Route
    {
        private static readonly string missingClusterId = new Error("Missing cluster id").ToString();
        private static readonly string invalidToken = new Error("Invalid token").ToString();
        private static readonly string invalidClusterId = new Error("Invalid Cluster id").ToString();
        private static readonly string unauthorizedAction = new Error("Unauthorized action").ToString();
        private static readonly string missingMemberId = new Error("Missing member id").ToString();
        private static readonly string missingMemberName = new Error("Missing member name").ToString();

        public Members(IServiceCollection services)
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

            return c.SerializeMembers();
        }

        protected override byte[] post(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string id = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            string name = request.QueryString.GetValues("name")?[0]
                ?? throw new HttpListenerException(400, missingMemberName);

            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            this.services.TokenManager.RefreshToken(token);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            string userId = this.services.TokenManager.GetIdFromToken(token);
            if (userId != c.owner && !c.admins.Contains(userId))
                throw new HttpListenerException(401, unauthorizedAction);

            c.members.Add(id, name);
            this.services.ClusterManager.Put(request.UrlParameters["id"], c);

            return new byte[] { };
        }

        protected override byte[] delete(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string id = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            this.services.TokenManager.RefreshToken(token);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            string userId = this.services.TokenManager.GetIdFromToken(token);
            if (userId != c.owner && !c.admins.Contains(userId))
                throw new HttpListenerException(401, unauthorizedAction);

            c.members.Remove(id);
            this.services.ClusterManager.Put(request.UrlParameters["id"], c);

            return new byte[] { };
        }
    }
}
