using System;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class JoinTests
    {
        private static readonly ServiceCollection services;
        private static readonly MockupRouter router;

        private string token;

        static JoinTests()
        {
            services = new ServiceCollection(new ClusterDatabase("../JoinTests"));
            router = new MockupRouter("/clusters/{id}/join", new Join(services));
        }

        public JoinTests()
        {
            this.token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(new Node(Guid.NewGuid().ToString(), router.ClientIp), this.token);
        }

        [Fact]
        public async void Post_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/clusters/id/join?username=username");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();
            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters//join?username=username")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenMissingUsername_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error($"Missing query parameter username").ToString();

            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters/id/join")
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
            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{invalidClusterId}/join?username=username")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenAlreadyMember_ShouldRespondWithOk()
        {
            var cluster = new Cluster("testcluster", services.TokenManager.GetNodeFromToken(this.token).id, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/join?username=hey")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void Post_WhenNotInvited_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Not allowed to join this cluster").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(new Node(ownerId, "[::1]:42"), token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/join?username=hey")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenInvited_ShouldRespondWithOk()
        {
            string expectedResponseContent = new Error("Not allowed to join this cluster").ToString();

            string ownerId = Guid.NewGuid().ToString();
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(new Node(ownerId, "[::1]:42"), token);
            var cluster = new Cluster("testcluster", ownerId, "ownername");
            cluster.invitedIdentities.Add(services.TokenManager.GetNodeFromToken(this.token).id);
            services.ClusterManager.Put(cluster);

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters/{cluster.id.ToString()}/join?username=hey")
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
            var response = await router.GetResponseOf(HttpMethod.Head, "/clusters/id/join");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldRespondWithNotFound()
        {
            string notFound = new Error("Not Found").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/clusters/id/join");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Put, "/clusters/id/join");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Delete, "/clusters/id/join");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Options, "/clusters/id/join");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);
        }
    }
}
