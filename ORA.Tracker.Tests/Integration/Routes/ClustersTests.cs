using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Text;

using ORA.Tracker.Services;
using ORA.Tracker.Models;
using ORA.Tracker.Services.Managers;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class ClustersTests
    {
        private static readonly MockupListener listener;
        private static readonly ServiceCollection services;

        private MockupClusters testee;
        private string token;

        static ClustersTests()
        {
            listener = new MockupListener(15302);
            services = new ServiceCollection(new ClusterDatabase("../ClustersTests"));
        }

        public ClustersTests()
        {
            this.testee = new MockupClusters(services);

            this.token = this.testee.Services.TokenManager.NewToken();
            if (!this.testee.Services.TokenManager.IsTokenRegistered(token))
                this.testee.Services.TokenManager.RegisterToken(Guid.NewGuid().ToString(), token);
        }

        [Fact]
        public async void WhenGetExistingCluster_ShouldMatch()
        {
            var c = new Cluster("test", Guid.NewGuid().ToString(), "ownerName");
            this.testee.Services.ClusterManager.Put(c.id.ToString(), c);

            var (request, response) = await listener.GetContext("/" + c.id.ToString(), HttpMethod.Get);
            testee.HandleRequest(request, response)
                .Should()
                .Equals(c.Serialize());
        }

        [Fact]
        public async void WhenGetInexistingCluster_ShouldThrow_HttpListenerException()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            var (request, response) = await listener.GetContext("/", new Dictionary<string, string> { { "id", inexistingId } }, HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidClusterId))
                .Where(e => e.ErrorCode.Equals(404));
        }

        [Fact]
        public async void WhenGetWithoutParameter_ShouldReturn_AllClusters()
        {
            var (request, response) = await listener.GetContext("/", HttpMethod.Get);
            testee.HandleRequest(request, response);

            // TODO: write test
        }

        [Fact]
        public async void WhenPostWithoutCredentials_ShouldThrow_HttpListenerException()
        {
            string missingCredentials = new Error("Missing Credentials").ToString();

            var (request, response) = await listener.GetContext("?name=name", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public async void WhenPost_ShouldCreateCluster()
        {
            string clusterName = "test";
            var (request, response) = await listener.GetContext($"?name={clusterName}&username=ownerName", HttpMethod.Post, token);

            string clusterId = Encoding.UTF8.GetString(testee.HandleRequest(request, response))
                .Split(":")[1].Split("\"")[1].Split("\"")[0];   // TODO: Do this more cleanly
            var c = this.testee.Services.ClusterManager.Get(clusterId);

            c.Should().BeOfType<Cluster>().Which.name.Should().Be(clusterName);
        }

        [Fact]
        public async void WhenPostWithoutNameParameter_ShouldThrow_HttpListenerException()
        {
            string missingNameParameter = new Error("Missing name Parameter").ToString();

            var (request, response) = await listener.GetContext("/", HttpMethod.Post, token);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingNameParameter))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenDeleteWithoutCredentials_ShouldThrow_HttpListenerException()
        {
            string missingCredentials = new Error("Missing Credentials").ToString();

            var (request, response) = await listener.GetContext("/", new Dictionary<string, string> { { "id", "id" } }, HttpMethod.Delete);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public async void WhenDeleteAndUnauthorized_ShouldThrow_HttpListenerException()
        {
            string unauthorizedAction = new Error("Unauthorized action").ToString();

            Cluster c = new Cluster("test", "notsameid", "ownerName");
            this.testee.Services.ClusterManager.Put(c.id.ToString(), c);

            var (request, response) = await listener.GetContext("/", new Dictionary<string, string> { { "id", c.id.ToString() } }, HttpMethod.Delete, token);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(unauthorizedAction))
                .Where(e => e.ErrorCode.Equals(403));
        }

        [Fact]
        public async void WhenDeleteExistingClusterAndAuthorized_ShouldReturn_EmptyBody()
        {
            Cluster c = new Cluster("test", this.testee.Services.TokenManager.GetIdFromToken(token), "ownerName");
            this.testee.Services.ClusterManager.Put(c.id.ToString(), c);

            var (request, response) = await listener.GetContext("/", new Dictionary<string, string> { { "id", c.id.ToString() } }, HttpMethod.Delete, token);
            testee.HandleRequest(request, response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenDeleteInexistingCluster_ShouldThrow_HttpListenerException()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            var (request, response) = await listener.GetContext("/", new Dictionary<string, string> { { "id", inexistingId } }, HttpMethod.Delete, token);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidClusterId))
                .Where(e => e.ErrorCode.Equals(404));
        }

        [Fact]
        public async void WhenDeleteWithoutClusterId_ShouldThrow_HttpListenerException()
        {
            string missingClusterId = new Error("Missing Cluster id").ToString();

            var (request, response) = await listener.GetContext("/", HttpMethod.Delete, token);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingClusterId))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            var (request, response) = await listener.GetContext("/", HttpMethod.Head);
            testee.HandleRequest(request, response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShoudlThrow_HttpListenerException()
        {
            string notFound = new Error("Not Found").ToString();

            var (request, response) = await listener.GetContext("/", HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            (request, response) = await listener.GetContext("/", HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));
        }
    }

    internal class MockupClusters : Clusters
    {
        public IServiceCollection Services { get => services; }

        public MockupClusters(IServiceCollection services)
            : base(services) { }
    }
}
