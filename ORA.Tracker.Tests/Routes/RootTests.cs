using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;
using ORA.Tracker.Tests.Utils;

namespace ORA.Tracker.Routes.Tests
{
    public class RootTests
    {
        private static readonly MockupListener listener = new MockupListener(15301);
        private static readonly string routePath = "/";

        [Fact]
        public async void WhenHandledMethodRequest_ShouldReturn_WelcomeMessage()
        {
            var testee = new Root();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(System.Text.Encoding.UTF8.GetBytes("Hey welcome to '/'"));
        }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            var testee = new Root();
            HttpListenerContext context;

            context = await listener.GenerateContext(routePath, HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldThrow_HttpListenerException()
        {
            var testee = new Root();
            HttpListenerContext context;

            string notFound = new Error("Not Found").ToString();

            context = await listener.GenerateContext(routePath, HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext(routePath, HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            context = await listener.GenerateContext(routePath, HttpMethod.Delete);
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
    }
}
