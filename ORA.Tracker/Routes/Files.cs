using System;
using System.Net;
using ORA.Tracker.Services;
using ORA.Tracker.Http;
using ORA.Tracker.Routes.Attributes;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Files : Route
    {
        private static readonly byte[] missingClusterId = new Error("Missing cluster id").ToBytes();
        private static readonly byte[] invalidClusterId = new Error("Invalid Cluster id").ToBytes();
        private static readonly byte[] unauthorizedAction = new Error("Unauthorized action").ToBytes();
        private static readonly byte[] notClusterFile = new Error("hash does not correspond to a cluster file").ToBytes();
        private static readonly byte[] invalidFile = new Error("This is not a valid file").ToBytes();

        public Files(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        [RequiredUrlParameters("id")]
        [RequiredQueryParameters("hash")]
        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string hash = request.QueryString["hash"];
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.HasMember(this.services.TokenManager.GetIdFromToken(token)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            if (!cluster.HasFile(hash))
            {
                response.NotFound(notClusterFile);
                return;
            }

            var file = cluster.GetFile(hash);

            response.Close(file.Serialize(), true);
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
        protected override void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            if (!cluster.HasMember(this.services.TokenManager.GetIdFromToken(token)))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            File file = null;

            try
            {
                file = File.Deserialize(request.Body);
            }
            catch (Exception)
            {
                response.BadRequest(invalidFile);
                return;
            }

            cluster.AddFile(file);
            this.services.ClusterManager.Put(cluster);

            response.Close();
        }

        [Authenticate]
        [RequiredUrlParameters("id")]
        [RequiredQueryParameters("hash")]
        protected override void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
        {
            string token = request.Token;
            string hash = request.QueryString["hash"];
            this.services.TokenManager.RefreshToken(token);

            var cluster = this.services.ClusterManager.Get(request.UrlParameters["id"]);
            if (cluster == null)
            {
                response.NotFound(invalidClusterId);
                return;
            }

            string userId = this.services.TokenManager.GetIdFromToken(token);
            if (userId != cluster.owner && !cluster.admins.Contains(userId))
            {
                response.Forbidden(unauthorizedAction);
                return;
            }

            cluster.RemoveFile(hash);
            this.services.ClusterManager.Put(cluster);

            response.Close();
        }
    }
}
