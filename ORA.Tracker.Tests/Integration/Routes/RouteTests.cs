using System;
using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;
using System.Text;

using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Tests.Integration.Routes
{
    public class RouteTests
    {
        private static readonly MockupListener listener = new MockupListener(15300);

        private MockRoute testee = new MockRoute();
        private HttpListenerContext context;

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            context = await listener.GenerateContext("/", HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldThrow_HttpListenerException()
        {
            string notFound = new Error("Not Found").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext("/", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext("/", HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext("/", HttpMethod.Delete);
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

        [Fact]
        public async void WhenGettingUrlParams_ShouldMatch()
        {
            var urlParams = new string[] { "it", "is", "a", "test" };
            string path = "/" + String.Join("/", urlParams);

            context = await listener.GenerateContext(path, HttpMethod.Get);
            testee.GetUrlParams(context.Request)
                .Should()
                .Equals(urlParams);
        }

        [Fact]
        public async void WhenGetBody_ShouldMatch()
        {
            byte[] content = Encoding.UTF8.GetBytes("It is a test content");

            context = await listener.GenerateContext("/", HttpMethod.Post, content);
            System.Text.Encoding.UTF8.GetString(testee.GetBody(context.Request))
                .Should()
                .Equals(content);
        }
    }

    internal class MockRoute : Route
    {
        public MockRoute()
            : base() { }

        public string[] GetUrlParams(HttpListenerRequest request) => this.getUrlParams(request);
        public byte[] GetBody(HttpListenerRequest request) => this.getBody(request);
    }
}
