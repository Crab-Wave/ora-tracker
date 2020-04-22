using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;
using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;

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

        public Members()
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

            return c.SerializeMembers();
        }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string id = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            string name = request.QueryString.GetValues("name")?[0]
                ?? throw new HttpListenerException(400, missingMemberName);

            if (urlParams == null || !urlParams.ContainsKey("id") || urlParams["id"] == "")
                throw new HttpListenerException(400, missingClusterId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            var c = ClusterDatabase.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            string userId = TokenManager.Instance.GetIdFromToken(token);
            if (userId != c.owner || !c.admins.Contains(userId))
                throw new HttpListenerException(403, unauthorizedAction);

            c.members.Add(id, name);
            ClusterDatabase.Put(urlParams["id"], c);

            return new byte[] { };
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
        {
            string token = Services.Authorization.GetToken(request.Headers);

            string id = request.QueryString.GetValues("id")?[0]
                ?? throw new HttpListenerException(400, missingMemberId);

            if (!TokenManager.Instance.IsValidToken(token))
                throw new HttpListenerException(400, invalidToken);

            TokenManager.Instance.RefreshToken(token);

            var c = ClusterDatabase.Get(urlParams["id"])
                ?? throw new HttpListenerException(404, invalidClusterId);
            string userId = TokenManager.Instance.GetIdFromToken(token);
            if (userId != c.owner || !c.admins.Contains(userId))
                throw new HttpListenerException(403, unauthorizedAction);

            c.members.Remove(id);
            ClusterDatabase.Put(urlParams["id"], c);

            return new byte[] { };
        }
    }
}
