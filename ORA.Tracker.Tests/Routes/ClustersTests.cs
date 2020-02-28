using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Tests.Utils;
using ORA.Tracker.Routes;

namespace ORA.Tracker.Routes.Tests
{
    public class ClustersTests
    {
        private static readonly MockupListener listener = new MockupListener(15302);
        private static readonly string routePath = "/clusters";

        [Fact]
        public async void WhenHandledMethodRequest_ShouldReturn_String()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);

            context = await listener.GenerateContext(routePath, HttpMethod.Post);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);

            context = await listener.GenerateContext(routePath, HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_Throws_HttpListenerException()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            string notFound = "{\n  \"message\": \"Not Found\",\n  \"documentation_url\": \"https://ora.crabwave.com/documentation\"\n}";

            context = await listener.GenerateContext(routePath, HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));

            context = await listener.GenerateContext(routePath, HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));
        }
    }
}
