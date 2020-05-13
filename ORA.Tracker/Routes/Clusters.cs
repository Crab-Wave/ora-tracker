using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        private static readonly string missingNameParameter = new Error("Missing name Parameter").ToString();
        private static readonly string missingUsernameParameter = new Error("Missing username Parameter").ToString();
        private static readonly string missingClusterId = new Error("Missing Cluster id").ToString();
        private static readonly string invalidClusterId = new Error("Invalid Cluster id").ToString();
        private static readonly string invalidToken = new Error("Invalid token").ToString();
        private static readonly string unauthorizedAction = new Error("Unauthorized action").ToString();

        public Clusters(IServiceCollection services)
            : base(services) { }

        protected override byte[] get(HttpRequest request, HttpListenerResponse response)
        {
            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
                return this.services.ClusterManager.GetAll();

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);

            return cluster.SerializeWithoutMemberName();
        }

        protected override byte[] post(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string name = request.QueryString.GetValues("name")?[0]
                ?? throw new HttpListenerException(400, missingNameParameter);

            string username = request.QueryString.GetValues("username")?[0]
                ?? throw new HttpListenerException(400, missingUsernameParameter);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            this.services.TokenManager.RefreshToken(token);

            Cluster cluster = new Cluster(name, this.services.TokenManager.GetIdFromToken(token), username);
            this.services.ClusterManager.Put(cluster.id.ToString(), cluster);

            return cluster.SerializeId();
        }

        protected override byte[] delete(HttpRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            if (request.UrlParameters == null || !request.UrlParameters.ContainsKey("id") || request.UrlParameters["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!this.services.TokenManager.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            this.services.TokenManager.RefreshToken(token);

            var c = this.services.ClusterManager.Get(request.UrlParameters["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (this.services.TokenManager.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(403, unauthorizedAction);

            this.services.ClusterManager.Delete(request.UrlParameters["id"]);
            return new byte[0];
        }
    }
}
