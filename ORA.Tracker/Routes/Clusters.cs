using System;
using System.Net;

using ORA.Tracker.Models;
using ORA.Tracker.Database;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        private static readonly string missingCredentials = new Error("Missing Credentials").ToString();
        private static readonly string missingNameParameter = new Error("Missing name Parameter").ToString();
        private static readonly string missingClusterId = new Error("Missing Cluster id").ToString();

        public Clusters()
            : base("/clusters") { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response)
        {
            var urlParams = this.getUrlParams(request);
            if (urlParams.Length < 1)
                return new byte[0];     // TODO: return all

            try
            {
                return DatabaseManager.Get(urlParams[0]);
            }
            catch (ArgumentNullException)
            {
                throw new Exception("Invalid cluster id");
            }
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            string[] authorizationValues = request.Headers.GetValues("Authorization");
            if (authorizationValues == null || authorizationValues.Length < 1)
                throw new HttpListenerException(401, missingCredentials);

            // TODO: perform authentication

            string[] nameValues = request.QueryString.GetValues("name");
            if (nameValues == null || nameValues.Length < 1)
                throw new HttpListenerException(400, missingNameParameter);

            Cluster cluster = new Cluster(nameValues[0], Guid.NewGuid());   // TODO: pass author guid
            DatabaseManager.Put(cluster.id.ToString(), cluster);

            return cluster.SerializeId();
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response)
        {
            string[] authorizationValues = request.Headers.GetValues("Authorization");
            if (authorizationValues == null || authorizationValues.Length < 1)
                throw new HttpListenerException(401, missingCredentials);

            // TODO: perfom authentication

            var urlParams = this.getUrlParams(request);
            if (urlParams.Length < 1)
                throw new HttpListenerException(400, missingClusterId);

            DatabaseManager.Delete(urlParams[0]);

            return new byte[0];
        }
    }
}
