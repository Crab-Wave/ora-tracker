using System;
using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Tests.Utils;

namespace ORA.Tracker.Routes.Tests
{
    public class RouteTests
    {
        private static readonly MockupListener listener = new MockupListener(15300);
        private static readonly string routePath = "/route";

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            var testee = new MockRoute();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_Throws_HttpListenerException()
        {
            var testee = new MockRoute();
            HttpListenerContext context;

            string notFound = "{\n  \"message\": \"Not Found\",\n  \"documentation_url\": \"https://ora.crabwave.com/documentation\"\n}";

            context = await listener.GenerateContext(routePath, HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));

            context = await listener.GenerateContext(routePath, HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));

            context = await listener.GenerateContext(routePath, HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Replace("\r", "").Equals(notFound));

            context = await listener.GenerateContext(routePath, HttpMethod.Delete);
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

        [Fact]
        public async void WhenGettingUrlParams_ShouldMatch()
        {
            var testee = new MockRoute();
            HttpListenerContext context;

            var urlParams = new string[] { "it", "is", "a", "test" };
            string path = routePath + "/" + String.Join("/", urlParams);

            context = await listener.GenerateContext(routePath, HttpMethod.Get);
            testee.GetUrlParams(context.Request)
                .Should()
                .Equals(urlParams);
        }
    }

    internal class MockRoute : Route
    {
        public MockRoute()
            : base("/route") { }

        public string[] GetUrlParams(HttpListenerRequest request) => this.getUrlParams(request);
    }
}
