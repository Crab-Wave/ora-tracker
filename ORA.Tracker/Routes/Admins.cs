using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;
using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;

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

        public Admins()
            : base() { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams = null)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            if (urlParams == null || !urlParams.ContainsKey("id") || urlParams["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            var c = ClusterDatabase.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (!c.members.ContainsKey(TokenManager.Instance.GetIdFromToken(token)))
                throw new HttpListenerException(403, unauthorizedAction);

            return c.SerializeAdmins();
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string id = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            if (urlParams == null || !urlParams.ContainsKey("id") || urlParams["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            var c = ClusterDatabase.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (TokenManager.Instance.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(403, unauthorizedAction);

            if (!c.members.ContainsKey(id))     // return error if is not member ?
                throw new HttpListenerException(400, notClusterMember);

            c.admins.Add(id);
            ClusterDatabase.Put(urlParams["id"], c);

            return new byte[] { };
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string adminId = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            var c = ClusterDatabase.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            if (TokenManager.Instance.GetIdFromToken(token) != c.owner)
                throw new HttpListenerException(403, unauthorizedAction);

            if (!c.admins.Contains(adminId))
                throw new HttpListenerException(400, notClusterAdmin);

            c.admins.Remove(adminId);
            ClusterDatabase.Put(urlParams["id"], c);

            return new byte[] { };
        }
    }
}
