using System;
using System.Net.Http;
using System.Linq;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class MembersTests
    {
        private static readonly ServiceCollection services;
        private static readonly MockupRouter router;

        private string token;

        static MembersTests()
        {
            services = new ServiceCollection(new ClusterDatabase("../MembersTests"));
            router = new MockupRouter("/clusters/{id}/members", new Members(services));
        }

        public MembersTests()
        {
            this.token = services.TokenManager.RegisterNode(Guid.NewGuid().ToString(), router.ClientIp);
        }

        [Fact]
        public async void Get_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/clusters/id/members");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Get, "/clusters//members")
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
            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{invalidClusterId}/members")
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
            string token = services.TokenManager.RegisterNode(ownerId, "[::1]:42");
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/members")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenMemberOfCluster_ShouldRespondWithListOfMembers()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromIp(router.ClientIp), "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Get, $"/clusters/{cluster.id.ToString()}/members")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEquivalentTo(cluster.SerializeMembers());
        }

        [Fact]
        public async void Post_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/clusters/id/members?id=id&name=name");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters//members?id=id&name=name")
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

            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters/id/members?name=name")
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
            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{invalidClusterId}/members?id=1234")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenNotOwnerOrAdminOfCluster_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.RegisterNode(ownerId, "[::1]:42");
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.members.Add(Guid.NewGuid().ToString(), this.token);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/members?id=123")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenOwnerOrAdminOfCluster_ShouldRespondWithOK()
        {
            string memberId = "memberid";
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromIp(router.ClientIp), "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/members?id={memberId}")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void Delete_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, "/clusters/id/members?id=id");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters//members?id=id")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingMemberId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error($"Missing query parameter id").ToString();

            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters/id/members")
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
            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{invalidClusterId}/members?id=1234")
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
            string token = services.TokenManager.RegisterNode(ownerId, "[::1]:42");
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.members.Add(Guid.NewGuid().ToString(), this.token);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/members?id=123")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenOwnerOrAdminOfCluster_ShouldRespondWithOK()
        {
            string memberId = "memberid";
            string memberName = "membername";
            var cluster = new Cluster("testcluster", services.TokenManager.GetIdFromIp(router.ClientIp), "ownername");
            cluster.members[memberId] = memberName;
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{cluster.id.ToString()}/members?id={memberId}")
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
            var response = await router.GetResponseOf(HttpMethod.Head, "/clusters/id/members");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldRespondWithNotFound()
        {
            string notFound = new Error("Not Found").ToString();

            var response = await router.GetResponseOf(HttpMethod.Put, "/clusters/id/members");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Options, "/clusters/id/members");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);
        }
    }
}
