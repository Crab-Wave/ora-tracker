using System;
using System.Text;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class FilesTests
    {
        private static readonly ServiceCollection services;
        private static readonly MockupRouter router;

        private string token;

        static FilesTests()
        {
            services = new ServiceCollection(new ClusterDatabase("../FilesTests"));
            router = new MockupRouter("/clusters/{id}/files", new Files(services));
        }

        public FilesTests()
        {
            this.token = services.TokenManager.NewToken();
            if (!services.TokenManager.IsTokenRegistered(token))
                services.TokenManager.RegisterToken(Guid.NewGuid().ToString(), token);
        }

        [Fact]
        public async void Get_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/clusters/id/files?hash=hash");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Get, "/clusters//files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenMissingHash_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing query parameter hash").ToString();
            var request = new MockupRouterRequest(HttpMethod.Get, "/clusters/id/files")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenInvalidClusterId_ShouldRespondWithNotFound()
        {
            string expectedResponseContent = new Error("Invalid Cluster id").ToString();
            string invalidClusterId = "invalidclusterid";
            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{invalidClusterId}/files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenNotMemberOfCluster_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(ownerId, token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenFileNotInCluster_ShouldRespondWithNotFound()
        {
            string expectedResponseContent = new Error("hash does not correspond to a cluster file").ToString();

            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromToken(this.token), "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenValidRequest_ShouldRespondFileInformation()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromToken(this.token), "ownername");
            string hash = "123141";
            cluster.AddFile(new File(hash, "ORA", 42));
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/files?hash={hash}")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEquivalentTo(cluster.GetFile(hash).Serialize());
        }

        [Fact]
        public async void Post_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters/id/files")
            {
                Body = Encoding.UTF8.GetBytes("{\"hash\":\"a121d1df\",\"path\":\"ORA\",\"size\":42}")
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters//files")
            {
                Credentials = this.token,
                Body = Encoding.UTF8.GetBytes("{\"hash\":\"a121d1df\",\"path\":\"ORA\",\"size\":42}")
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenInvalidClusterId_ShouldRespondWithNotFound()
        {
            string expectedResponseContent = new Error("Invalid Cluster id").ToString();
            string invalidClusterId = "invalidclusterid";
            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{invalidClusterId}/files")
            {
                Credentials = this.token,
                Body = Encoding.UTF8.GetBytes("{\"hash\":\"a121d1df\",\"path\":\"ORA\",\"size\":42}")
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenNotMember_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(ownerId, token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.members.Add(Guid.NewGuid().ToString(), this.token);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/files")
            {
                Credentials = this.token,
                Body = Encoding.UTF8.GetBytes("{\"hash\":\"a121d1df\",\"path\":\"ORA\",\"size\":42}")
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenInvalidBody_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("This is not a valid file").ToString();

            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromToken(this.token), "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/files")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenValid_ShouldRespondWithOK()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromToken(this.token), "ownername");
            services.ClusterManager.Put(cluster);
            var file = new File("a121d1df", "ORA", 42);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/files")
            {
                Credentials = this.token,
                Body = file.Serialize()
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
            services.ClusterManager.Get(cluster.id.ToString()).GetFile(file.hash).Should().BeEquivalentTo(file);
        }

        [Fact]
        public async void Delete_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, "/clusters/id/files?hash=hash");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters//files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingHash_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error($"Missing query parameter hash").ToString();

            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters/id/files")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenInvalidClusterId_ShouldRespondWithNotFound()
        {
            string expectedResponseContent = new Error("Invalid Cluster id").ToString();
            string invalidClusterId = "invalidclusterid";
            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{invalidClusterId}/files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenNotOwnerOrAdminOfCluster_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(ownerId, token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.members.Add(Guid.NewGuid().ToString(), this.token);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenInvalidHash_ShouldRespondWithOK()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromToken(this.token), "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/files?hash=hash")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
            services.ClusterManager.Get(cluster.id.ToString()).HasFile("hash").Should().BeFalse();
        }

        [Fact]
        public async void Delete_WhenValidHash_ShouldRespondWithOK()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromToken(this.token), "ownername");
            var file = new File("afd123", "ORA", 42);
            cluster.AddFile(file);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/files?hash={file.hash}")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
            services.ClusterManager.Get(cluster.id.ToString()).HasFile(file.hash).Should().BeFalse();
        }

        [Fact]
        public async void WhenHeadRequest_ShouldRespondWithNotFoundAndEmptyBody()
        {
            var response = await router.GetResponseOf(HttpMethod.Head, "/");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldRespondWithNotFound()
        {
            string notFound = new Error("Not Found").ToString();

            var response = await router.GetResponseOf(HttpMethod.Put, "/");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Options, "/");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);
        }
    }
}
