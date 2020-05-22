using System;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;
using ORA.Tracker.Models;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class ClustersTests
    {
        private static readonly ServiceCollection services;
        private static readonly MockupRouter router;

        private string token;

        static ClustersTests()
        {
            services = new ServiceCollection(new ClusterDatabase("../ClustersTests"));
            router = new MockupRouter("/clusters/{id}", new MockupClusters(services));
        }

        public ClustersTests()
        {
            this.token = services.TokenManager.NewToken();
            if (!services.TokenManager.IsTokenRegistered(token))
                services.TokenManager.RegisterToken(Guid.NewGuid().ToString(), token);
        }

        [Fact]
        public async void Get_WhenExistingCluster_ShouldWithMatchingCluster()
        {
            var c = new Cluster("test", Guid.NewGuid().ToString(), "ownerName");
            services.ClusterManager.Put(c);

            var response = await router.GetResponseOf(HttpMethod.Get, $"/clusters/{c.id.ToString()}");

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEquivalentTo(c.SerializeWithoutMemberName());
        }

        [Fact]
        public async void Get_WhenInexistingCluster_ShouldRespondWithNotFound()
        {
            string inexistingId = "test";
            string expectedResponseContent = new Error("Invalid Cluster id").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, $"/clusters/{inexistingId}");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Get_WhenNoIdParameter_ShouldRespondWithAllClusters()
        {
            var response = await router.GetResponseOf(HttpMethod.Get, "/clusters");

            response.StatusCode.Should().Be(200);

            // TODO: write test
        }

        [Fact]
        public async void Post_WhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/clusters?name=name");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Post_WhenValidCredentials_ShouldCreateClusterAndRespondWithClusterId()
        {
            string clusterName = "test";

            var request = new MockupRouterRequest(HttpMethod.Post, $"/clusters?name={clusterName}&username=ownername")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            string clusterId = response.Content.ReadAsStringAsync().Result.Split(":")[1].Split("\"")[1].Split("\"")[0];   // TODO: Do this more cleanly
            var c = services.ClusterManager.Get(clusterId);

            c.Should().BeOfType<Cluster>().Which.name.Should().Be(clusterName);
        }

        [Fact]
        public async void Post_WhenMissingNameParameter_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing query parameter name").ToString();

            var request = new MockupRouterRequest(HttpMethod.Post, "/clusters")
            {
                Credentials = this.token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenWhenMissingCredentials_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, "/clusters/id");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenExistingClusterAndUnauthorized_ShouldRespondWithForbidden()
        {
            string expectedResponseContent = new Error("Unauthorized action").ToString();

            Cluster c = new Cluster("test", "notsameid", "ownerName");
            services.ClusterManager.Put(c);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{c.id.ToString()}")
            {
                Credentials = token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenExistingClusterAndAuthorized_ShouldRespondWithEmptyBody()
        {
            Cluster c = new Cluster("test", services.TokenManager.GetIdFromToken(token), "ownerName");
            services.ClusterManager.Put(c);

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{c.id.ToString()}")
            {
                Credentials = token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void Delete_WhenInexistingCluster_ShouldRespondWithNotFound()
        {
            string inexistingId = "test";
            string expectedResponseContent = new Error("Invalid Cluster id").ToString();

            var request = new MockupRouterRequest(HttpMethod.Delete, $"/clusters/{inexistingId}")
            {
                Credentials = token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Delete_WhenMissingClusterId_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing url parameter id").ToString();

            var request = new MockupRouterRequest(HttpMethod.Delete, "/clusters")
            {
                Credentials = token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void WhenHeadRequest_ShouldRespondWithNotFoundAndEmptyBody()
        {
            var response = await router.GetResponseOf(HttpMethod.Head, "/clusters");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldRespondWithNotFound()
        {
            string notFound = new Error("Not Found").ToString();

            var response = await router.GetResponseOf(HttpMethod.Put, "/clusters");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Options, "/clusters");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);
        }
    }

    internal class MockupClusters : Clusters
    {
        public IServiceCollection Services { get => services; }

        public MockupClusters(IServiceCollection services)
            : base(services) { }
    }
}
