using System;
using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;
using ORA.Tracker.Database;
using ORA.Tracker.Tests.Utils;

namespace ORA.Tracker.Routes.Tests
{
    public class ClustersTests
    {
        private static readonly MockupListener listener = new MockupListener(15302);
        private static readonly string routePath = "/clusters";

        // [Fact]
        // public async void WhenGettingExistingCluster_ShouldMatch()
        // {
        //     ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
        //     var testee = new Clusters();
        //     HttpListenerContext context;

        //     Cluster c = new Cluster("test", Guid.NewGuid());
        //     DatabaseManager.Put(c.id.ToString(), c);

        //     context = await listener.GenerateContext(routePath + "/" + c.id.ToString(), HttpMethod.Get);
        //     testee.HandleRequest(context.Request, context.Response)
        //         .Should()
        //         .Equals(c.Serialize());
        // }

        [Fact]
        public async void WhenGettingInexistingCluster_ShouldThrow_HttpListenerException()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            string inexistingId = "test";
            string invalidClusterId = new Error("Invalid Cluster id").ToString();

            context = await listener.GenerateContext(routePath + "/" + inexistingId, HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidClusterId))
                .Where(e => e.ErrorCode.Equals(404));
        }

        [Fact]
        public async void WhenGettingClusterWithoutParameter_ShouldReturn_AllClusters()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response);

            // TODO: write test
        }

        [Fact]
        public async void WhenCreatingCluster()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath + "?name=test", HttpMethod.Post);
            testee.HandleRequest(context.Request, context.Response);

            // TODO: write test
        }

        [Fact]
        public async void WhenCreatingClusterWithoutNameParameter_ShouldThrow_HttpListenerException()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            string missingNameParameter = new Error("Missing name Parameter").ToString();

            context = await listener.GenerateContext(routePath, HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingNameParameter))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenDeletingClusterExistingCluster_ShouldReturn_EmptyBody()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            Cluster c = new Cluster("test", Guid.NewGuid());
            DatabaseManager.Put(c.id.ToString(), c);

            context = await listener.GenerateContext(routePath + "/" + c.id.ToString(), HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenDeletingClusterInexistingCluster_ShouldReturn_EmptyBody()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            string inexistingId = "test";

            context = await listener.GenerateContext(routePath + "/" + inexistingId, HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        // [Fact]
        // public async void WhenDeletingClusterWithoutClusterId_ShouldThrow_HttpListenerException()
        // {
        //     ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
        //     var testee = new Clusters();
        //     HttpListenerContext context;

        //     string missingClusterId = new Error("Missing Cluster id").ToString();

        //     context = await listener.GenerateContext(routePath, HttpMethod.Delete);
        //     testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
        //         .Should()
        //         .Throw<HttpListenerException>()
        //         .Where(e => e.Message.Equals(missingClusterId))
        //         .Where(e => e.ErrorCode.Equals(400));
        // }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShoudlThrow_HttpListenerException()
        {
            ignoreErrors(() => DatabaseManager.Init("../DatabaseTest"));
            var testee = new Clusters();
            HttpListenerContext context;

            string notFound = new Error("Not Found").ToString();

            context = await listener.GenerateContext(routePath, HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext(routePath, HttpMethod.Options);
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
