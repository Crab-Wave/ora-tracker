using System;
using System.Net;
using System.Text;

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

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response)
        {
            var urlParams = this.getUrlParams(request);
            if (urlParams.Length < 1 || urlParams[0] == "")
                return ClusterDatabase.GetAll();

            try
            {
                return ClusterDatabase.Get(urlParams[0]);
            }
            catch (ArgumentNullException)
            {
                throw new HttpListenerException(404, invalidClusterId);
            }
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] token = Encoding.UTF8.GetBytes(Services.Authorization.GetToken(request.Headers));

            string[] nameValues = request.QueryString.GetValues("name");
            if (nameValues == null || nameValues.Length < 1)
                throw new HttpListenerException(400, missingNameParameter);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            Cluster cluster = new Cluster(nameValues[0], TokenManager.Instance.GetIdFromToken(token));
            ClusterDatabase.Put(cluster.id.ToString(), cluster);

            return cluster.SerializeId();
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response)
        {
            byte[] token = Encoding.UTF8.GetBytes(Services.Authorization.GetToken(request.Headers));

            var urlParams = this.getUrlParams(request);
            if (urlParams.Length < 1 || urlParams[0] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            try
            {
                var c = Cluster.Deserialize(ClusterDatabase.Get(urlParams[0]));

                if (TokenManager.Instance.GetIdFromToken(token) != c.owner)
                    throw new HttpListenerException(401, unauthorizedAction);

                // if cluster exists
                ClusterDatabase.Delete(urlParams[0]);
                return new byte[0];
            }
            catch (ArgumentNullException)
            {
                throw new HttpListenerException(404, invalidClusterId);
            }
        }
    }
}
