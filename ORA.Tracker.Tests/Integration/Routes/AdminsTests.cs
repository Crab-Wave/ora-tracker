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
            if (!services.TokenManager.IsTokenRegistered(token))
                services.TokenManager.RegisterToken(Guid.NewGuid().ToString(), token);
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
            string expectedResponseContent = new Error("Missing cluster id").ToString();
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
            services.TokenManager.RegisterToken(ownerId, token);
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
