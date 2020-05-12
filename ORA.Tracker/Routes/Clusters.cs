using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;
using ORA.Tracker.Services;

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

        public Clusters()
            : base() { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            if (urlParams == null || !urlParams.ContainsKey("id") || urlParams["id"] == "")
                return ClusterManager.Instance.GetAll();

            var cluster = ClusterManager.Instance.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);

            return cluster.SerializeWithoutMemberName();
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams = null)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string name = request.QueryString.GetValues("name")?[0]
                ?? throw new HttpListenerException(400, missingNameParameter);

            string username = request.QueryString.GetValues("username")?[0]
                ?? throw new HttpListenerException(400, missingUsernameParameter);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            Cluster cluster = new Cluster(name, TokenManager.Instance.GetIdFromToken(token), username);
            ClusterManager.Instance.Put(cluster.id.ToString(), cluster);

            return cluster.SerializeId();
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            if (urlParams == null || !urlParams.ContainsKey("id") || urlParams["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            var c = ClusterManager.Instance.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (TokenManager.Instance.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(403, unauthorizedAction);

            ClusterManager.Instance.Delete(urlParams["id"]);
            return new byte[0];
        }
    }
}
