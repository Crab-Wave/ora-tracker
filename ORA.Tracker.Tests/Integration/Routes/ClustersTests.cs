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
        private static readonly MockupRouter router;
        private static readonly ServiceCollection services;

        private MockupClusters testee;
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
        public async void WhenGetExistingCluster_ShouldWithMatchingCluster()
        {
            var c = new Cluster("test", Guid.NewGuid().ToString(), "ownerName");
            services.ClusterManager.Put(c.id.ToString(), c);

            var response = await router.GetResponseOf(HttpMethod.Get, $"/clusters/{c.id.ToString()}");

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEquivalentTo(c.SerializeWithoutMemberName());
        }

        [Fact]
        public async void WhenGetInexistingCluster_ShouldRespondWithNotFound()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, $"/clusters/{inexistingId}");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(invalidClusterId);
        }

        [Fact]
        public async void WhenGetWithoutParameter_ShouldRespondWithAllClusters()
        {
            var response = await router.GetResponseOf(HttpMethod.Get, "/clusters");

            response.StatusCode.Should().Be(200);

            // TODO: write test
        }

        [Fact]
        public async void WhenPostWithoutCredentials_ShouldRespondWithBadRequest()
        {
            string missingCredentials = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/clusters?name=name");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(missingCredentials);
        }

        [Fact]
        public async void WhenPost_ShouldCreateClusterAndRespondWithClusterId()
        {
            string clusterName = "test";
            var response = await router.GetResponseOf(HttpMethod.Post, $"/clusters?name={clusterName}&username=ownername", null, this.token);

            string clusterId = response.Content.ReadAsStringAsync().Result.Split(":")[1].Split("\"")[1].Split("\"")[0];   // TODO: Do this more cleanly
            var c = services.ClusterManager.Get(clusterId);

            c.Should().BeOfType<Cluster>().Which.name.Should().Be(clusterName);
        }

        [Fact]
        public async void WhenPostWithoutNameParameter_ShouldRespondWithBadRequest()
        {
            string missingNameParameter = new Error("Missing query parameter name").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/clusters", null, token);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(missingNameParameter);
        }

        [Fact]
        public async void WhenDeleteWithoutCredentials_ShouldRespondWithBadRequest()
        {
            string missingCredentials = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, "/clusters/id");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(missingCredentials);
        }

        [Fact]
        public async void WhenDeleteAndUnauthorized_ShouldRespondWithForbidden()
        {
            string unauthorizedAction = new Error("Unauthorized action").ToString();

            Cluster c = new Cluster("test", "notsameid", "ownerName");
            services.ClusterManager.Put(c.id.ToString(), c);

            var response = await router.GetResponseOf(HttpMethod.Delete, $"/clusters/{c.id.ToString()}", null, token);

            response.StatusCode.Should().Be(403);
            response.Content.ReadAsStringAsync().Result.Should().Be(unauthorizedAction);
        }

        [Fact]
        public async void WhenDeleteExistingClusterAndAuthorized_ShouldRespondWithEmptyBody()
        {
            Cluster c = new Cluster("test", services.TokenManager.GetIdFromToken(token), "ownerName");
            services.ClusterManager.Put(c.id.ToString(), c);

            var response = await router.GetResponseOf(HttpMethod.Delete, $"/clusters/{c.id.ToString()}", null, token);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenDeleteInexistingCluster_ShouldRespondWithNotFound()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, $"/clusters/{inexistingId}", null, token);

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(invalidClusterId);
        }

        [Fact]
        public async void WhenDeleteWithoutClusterId_ShouldRespondWithBadRequest()
        {
            string missingClusterId = new Error("Missing Cluster id").ToString();

            var response = await router.GetResponseOf(HttpMethod.Delete, "/clusters", null, token);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(missingClusterId);
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
