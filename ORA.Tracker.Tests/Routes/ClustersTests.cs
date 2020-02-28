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
        private static readonly HttpClient client = new HttpClient();
        private static readonly string routePath = "/clusters";

        [Fact]
        public async void WhenHandledMethodRequest_ShouldReturn_String()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            context = await Generator.GenerateListenerContext(routePath, HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Be("");

            context = await Generator.GenerateListenerContext(routePath, HttpMethod.Post);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Be("");

            context = await Generator.GenerateListenerContext(routePath, HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Be("");
        }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            context = await Generator.GenerateListenerContext(routePath, HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Be("");
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_Throws_HttpListenerException()
        {
            var testee = new Clusters();
            HttpListenerContext context;

            string notFound = "{\n  \"message\": \"Not Found\",\n  \"documentation_url\": \"https://ora.crabwave.com/documentation\"\n}";

            context = await Generator.GenerateListenerContext(routePath, HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));

            context = await Generator.GenerateListenerContext(routePath, HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));
        }
    }
}
