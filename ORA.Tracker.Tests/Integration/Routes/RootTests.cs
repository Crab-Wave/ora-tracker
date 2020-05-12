using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;
using System.Text;

using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class RootTests
    {
        private static readonly MockupListener listener = new MockupListener(15301);

        private Root testee = new Root();
        private HttpListenerContext context;

        [Fact]
        public async void WhenGet_ShouldReturn_WelcomeMessage()
        {
            context = await listener.GenerateContext("/", HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Equals(Encoding.UTF8.GetBytes("Hey welcome to '/'"));
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
        public async void WhenUnhandledMethodRequest_ShouldThrow_HttpListenerException()
        {
            string notFound = new Error("Not Found").ToString();

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
    }
}
