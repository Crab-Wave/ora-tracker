using System;
using System.Net;

using ORA.Tracker.Models;
using ORA.Tracker.Services;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        private static readonly string missingNameParameter = new Error("Missing name Parameter").ToString();
        private static readonly string missingClusterId = new Error("Missing Cluster id").ToString();
        private static readonly string invalidClusterId = new Error("Invalid Cluster id").ToString();

        public Clusters()
            : base() { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response)
        {
            var urlParams = this.getUrlParams(request);
            if (urlParams.Length < 1 || urlParams[0] == "")
                return Database.GetAll();

            try
            {
                return Database.Get(urlParams[0]);
            }
            catch (ArgumentNullException)
            {
                throw new HttpListenerException(404, invalidClusterId);
            }
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string[] nameValues = request.QueryString.GetValues("name");
            if (nameValues == null || nameValues.Length < 1)
                throw new HttpListenerException(400, missingNameParameter);

            Cluster cluster = new Cluster(nameValues[0], Guid.NewGuid());   // TODO: pass author guid
            Database.Put(cluster.id.ToString(), cluster);

            return cluster.SerializeId();
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            var urlParams = this.getUrlParams(request);
            if (urlParams.Length < 1 || urlParams[0] == "")
                throw new HttpListenerException(400, missingClusterId);

            try
            {
                Database.Get(urlParams[0]);

                // if cluster exists
                Database.Delete(urlParams[0]);
                return new byte[0];
            }
            catch (ArgumentNullException)
            {
                throw new HttpListenerException(404, invalidClusterId);
            }
        }
    }
}
