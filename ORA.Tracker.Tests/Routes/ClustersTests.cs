using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using System.Text;

using ORA.Tracker.Models;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Services;
using ORA.Tracker.Tests.Utils;

namespace ORA.Tracker.Routes.Tests
{
    public class ClustersTests
    {
        private static readonly MockupListener listener = new MockupListener(15302);

        private Clusters testee = new Clusters();
        private HttpListenerContext context;
        private string token;

        public ClustersTests()
        {
            ignoreErrors(() => ClusterDatabase.Init("../DatabaseTest"));

            token = TokenManager.Instance.NewToken();
            if (!TokenManager.Instance.IsTokenRegistered(token))
                TokenManager.Instance.RegisterToken(Guid.NewGuid().ToString(), token);
        }

        [Fact]
        public async void WhenGetExistingCluster_ShouldMatch()
        {
            var c = new Cluster("test", Guid.NewGuid().ToString());
            ClusterDatabase.Put(c.id.ToString(), c);

            context = await listener.GenerateContext("/" + c.id.ToString(), HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(c.Serialize());
        }

        [Fact]
        public async void WhenGetInexistingCluster_ShouldThrow_HttpListenerException()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response,
                    new Dictionary<string, string> { { "id", inexistingId } }))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidClusterId))
                .Where(e => e.ErrorCode.Equals(404));
        }

        [Fact]
        public async void WhenGetWithoutParameter_ShouldReturn_AllClusters()
        {
            context = await listener.GenerateContext("/", HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response);

            // TODO: write test
        }

        [Fact]
        public async void WhenPostWithoutCredentials_ShouldThrow_HttpListenerException()
        {
            string missingCredentials = new Error("Missing Credentials").ToString();

            context = await listener.GenerateContext("?name=name", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public async void WhenPost_ShouldCreateCluster()
        {
            string clusterName = "test";
            context = await listener.GenerateContext($"?name={clusterName}", HttpMethod.Post, token);

            string clusterId = Encoding.UTF8.GetString(testee.HandleRequest(context.Request, context.Response))
                .Split(":")[1].Split("\"")[1].Split("\"")[0];   // TODO: Do this more cleanly
            var c = ClusterDatabase.Get(clusterId);

            c.Should().BeOfType<Cluster>().Which.name.Should().Be(clusterName);
        }

        [Fact]
        public async void WhenPostWithoutNameParameter_ShouldThrow_HttpListenerException()
        {
            string missingNameParameter = new Error("Missing name Parameter").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Post, token);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingNameParameter))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenDeleteWithoutCredentials_ShouldThrow_HttpListenerException()
        {
            string missingCredentials = new Error("Missing Credentials").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Delete);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response,
                    new Dictionary<string, string> { { "id", "id" } }))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingCredentials))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public async void WhenDeleteAndUnauthorized_ShouldThrow_HttpListenerException()
        {
            string unauthorizedAction = new Error("Unauthorized action").ToString();

            Cluster c = new Cluster("test", "notsameid");
            ClusterDatabase.Put(c.id.ToString(), c);

            context = await listener.GenerateContext("/", HttpMethod.Delete, token);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response,
                    new Dictionary<string, string> { { "id", c.id.ToString() } }))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(unauthorizedAction))
                .Where(e => e.ErrorCode.Equals(401));
        }

        [Fact]
        public async void WhenDeleteExistingClusterAndAuthorized_ShouldReturn_EmptyBody()
        {
            Cluster c = new Cluster("test", TokenManager.Instance.GetIdFromToken(token));
            ClusterDatabase.Put(c.id.ToString(), c);

            context = await listener.GenerateContext("/", HttpMethod.Delete, token);
            testee.HandleRequest(context.Request, context.Response,
                    new Dictionary<string, string> { { "id", c.id.ToString() } })
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenDeleteInexistingCluster_ShouldThrow_HttpListenerException()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Delete, token);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response,
                    new Dictionary<string, string> { { "id", inexistingId } }))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidClusterId))
                .Where(e => e.ErrorCode.Equals(404));
        }

        [Fact]
        public async void WhenDeleteWithoutClusterId_ShouldThrow_HttpListenerException()
        {
            string missingClusterId = new Error("Missing Cluster id").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Delete, token);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingClusterId))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            context = await listener.GenerateContext("/", HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShoudlThrow_HttpListenerException()
        {
            string notFound = new Error("Not Found").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext("/", HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));
        }

        private static void ignoreErrors(Action f)
        {
            try
            {
                f();
            }
            catch { }
        }
    }
}
