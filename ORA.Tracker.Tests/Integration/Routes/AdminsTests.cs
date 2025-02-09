using System;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;
using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class AdminsTests
    {
        private static readonly ServiceCollection services;
        private static readonly MockupRouter router;

        private string token;

        static AdminsTests()
        {
            services = new ServiceCollection(new ClusterDatabase("../AdminsTests"));
            router = new MockupRouter("/clusters/{id}/admins", new Admins(services));
        }

        public AdminsTests()
        {
            this.token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(new Node(Guid.NewGuid().ToString(), "::1"), this.token);
        }

        [Fact]
        public async void Get_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/clusters/id/admins");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Get, "/clusters//admins")
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
            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{invalidClusterId}/admins")
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
            services.TokenManager.RegisterToken(new Node(ownerId, "[]::1]:42"), token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/admins")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenMemberOfCluster_ShouldRespondWithListOfAdmins()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetNodeFromToken(this.token).id, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/admins")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEquivalentTo(cluster.SerializeAdmins());
        }

        [Fact]
        public async void Post_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/clusters/id/admins?id=id");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters//admins")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenMissingId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error($"Missing query parameter id").ToString();

            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters/id/admins")
            {
                Credentials = this.token
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
            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{invalidClusterId}/admins?id=1234")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenNotOwnerOfCluster_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(new Node(ownerId, "[::1]:42"), token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.members.Add(Guid.NewGuid().ToString(), this.token);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/admins?id=123")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenIdIsNotMemberOfCluster_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("id does not correspond to a cluster member").ToString();

            var cluster = new Cluster("testcluster", services.TokenManager.GetNodeFromToken(this.token).id, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/admins?id=123")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenOwner_ShouldRespondWithOK()
        {
            string memberId = "memberid";
            var cluster = new Cluster("testcluster", services.TokenManager.GetNodeFromToken(this.token).id, "ownername");
            cluster.members.Add(memberId, "memberToken");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/admins?id={memberId}")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
            services.ClusterManager.Get(cluster.id.ToString()).admins.Should().Contain(a => a == memberId);
        }

        [Fact]
        public async void Delete_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, "/clusters/id/admins?id=id");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters//admins")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error($"Missing query parameter id").ToString();

            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters/id/admins")
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
            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{invalidClusterId}/admins?id=1234")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenNotOwnerOfCluster_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(new Node(ownerId, "[::1]:42"), token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.members.Add(Guid.NewGuid().ToString(), this.token);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/admins?id=123")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenIdIsNotAdminOfCluster_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("id does not correspond to a cluster admin").ToString();

            var cluster = new Cluster("testcluster", services.TokenManager.GetNodeFromToken(this.token).id, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/admins?id=123")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenOwner_ShouldRespondWithOK()
        {
            string memberId = "memberid";
            var cluster = new Cluster("testcluster", services.TokenManager.GetNodeFromToken(this.token).id, "ownername");
            cluster.admins.Add(memberId);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/admins?id={memberId}")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenHeadRequest_ShouldRespondWithNotFoundAndEmptyBody()
        {
            var response = await router.GetResponseOf(HttpMethod.Head, "/clusters/id/admins");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldRespondWithNotFound()
        {
            string notFound = new Error("Not Found").ToString();

            var response = await router.GetResponseOf(HttpMethod.Put, "/clusters/id/admins");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Options, "/clusters/id/admins");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);
        }
    }
}
