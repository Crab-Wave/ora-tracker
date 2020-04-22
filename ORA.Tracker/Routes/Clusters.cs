using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Services;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        private static readonly string missingNameParameter = new Error("Missing name Parameter").ToString();
        private static readonly string missingClusterId = new Error("Missing Cluster id").ToString();
        private static readonly string invalidClusterId = new Error("Invalid Cluster id").ToString();
        private static readonly string invalidToken = new Error("Invalid token").ToString();
        private static readonly string unauthorizedAction = new Error("Unauthorized action").ToString();

        public Clusters()
            : base() { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            if (urlParams == null || !urlParams.ContainsKey("id") || urlParams["id"] == "")
                return ClusterDatabase.GetAll();

            var clusterJsonBytes = ClusterDatabase.Get(urlParams["id"]);
            if (clusterJsonBytes == null)
                throw new HttpListenerException(404, invalidClusterId);

            return clusterJsonBytes;
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams = null)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string[] nameValues = request.QueryString.GetValues("name");
            if (nameValues == null || nameValues.Length < 1)
                throw new HttpListenerException(400, missingNameParameter);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            Cluster cluster = new Cluster(nameValues[0], TokenManager.Instance.GetIdFromToken(token));
            ClusterDatabase.Put(cluster.id.ToString(), cluster);

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

            var clusterJsonBytes = ClusterDatabase.Get(urlParams["id"]);
            if (clusterJsonBytes == null)
                throw new HttpListenerException(404, invalidClusterId);

            var c = Cluster.Deserialize(clusterJsonBytes);
            if (TokenManager.Instance.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(401, unauthorizedAction);

            ClusterDatabase.Delete(urlParams["id"]);
            return new byte[0];
        }
    }
}
