using System;
using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;
using System.Text;

using ORA.Tracker.Models;
using ORA.Tracker.Services.Databases;
using ORA.Tracker.Tests.Utils;

namespace ORA.Tracker.Routes.Tests
{
    public class ClustersTests
    {
        private static readonly MockupListener listener = new MockupListener(15302);

        private Clusters testee = new Clusters();
        private HttpListenerContext context;

        public ClustersTests()
        {
            ignoreErrors(() => ClusterDatabase.Init("../DatabaseTest"));
        }

        [Fact]
        public async void WhenGetExistingCluster_ShouldMatch()
        {
            ignoreErrors(() => ClusterDatabase.Init("../DatabaseTest"));

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

            context = await listener.GenerateContext("/" + inexistingId, HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
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
        public async void WhenPost_ShouldCreateCluster()
        {
            ignoreErrors(() => ClusterDatabase.Init("../DatabaseTest"));

            string clusterName = "test";
            context = await listener.GenerateContext($"?name={clusterName}", HttpMethod.Post);

            string clusterId = Encoding.UTF8.GetString(testee.HandleRequest(context.Request, context.Response))
                .Split(":")[1].Split("\"")[1].Split("\"")[0];   // TODO: Do this more cleanly
            var c = Cluster.Deserialize(ClusterDatabase.Get(clusterId));

            c.Should().BeOfType<Cluster>().Which.name.Should().Be(clusterName);
        }

        [Fact]
        public async void WhenPostWithoutNameParameter_ShouldThrow_HttpListenerException()
        {
            string missingNameParameter = new Error("Missing name Parameter").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingNameParameter))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenDeleteExistingCluster_ShouldReturn_EmptyBody()
        {
            ignoreErrors(() => ClusterDatabase.Init("../DatabaseTest"));

            var testee = new Clusters();
            HttpListenerContext context;

            Cluster c = new Cluster("test", Guid.NewGuid().ToString());
            ClusterDatabase.Put(c.id.ToString(), c);

            context = await listener.GenerateContext("/" + c.id.ToString(), HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenDeleteInexistingCluster_ShouldThrow_HttpListenerException()
        {
            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            context = await listener.GenerateContext("/" + inexistingId, HttpMethod.Delete);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidClusterId))
                .Where(e => e.ErrorCode.Equals(404));
        }

        [Fact]
        public async void WhenDeleteWithoutClusterId_ShouldThrow_HttpListenerException()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            string missingClusterId = new Error("Missing Cluster id").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Delete);
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
