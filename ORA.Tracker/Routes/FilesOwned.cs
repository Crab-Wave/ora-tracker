using System;
using System.Net;
using System.Text.Json;
using System.Collections.Generic;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Routes.Attributes;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class FilesOwned : Route
    {
        private static readonly byte[] invalidClusterId = new Error("Invalid Cluster id").ToBytes();
        private static readonly byte[] unauthorizedAction = new Error("Unauthorized action").ToBytes();
        private static readonly byte[] invalidFormat = new Error("Invalid body format").ToBytes();

        public FilesOwned(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        [RequiredUrlParameters("id")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string id = this.services.TokenManager.GetIdFromIp(request.Ip);
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.HasMember(id))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            HashSet<string> filesOwned = null;
            try
            {
                filesOwned = JsonSerializer.Deserialize<HashSet<string>>(request.Body);
            }
            catch (Exception)
            {
                response.BadRequest(invalidFormat);
                return;
            }

            this.services.NodeManager.AddFilesOwned(id, filesOwned);

            response.Close();
        }
    }
}
