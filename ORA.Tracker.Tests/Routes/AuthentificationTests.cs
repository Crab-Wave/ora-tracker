using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;
using System.Text;

using ORA.Tracker.Models;
using ORA.Tracker.Tests.Utils;

namespace ORA.Tracker.Routes.Tests
{
    public class AuthentificationTests
    {
        private static readonly MockupListener listener = new MockupListener(15304);

        private Authentification testee = new Authentification();
        private HttpListenerContext context;

        [Fact]
        public async void WhenPostWithoutParameter_ShouldThrow_HttpListenerException()
        {
            string missingIdParameter = new Error("Missing id parameter").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingIdParameter))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithoutIdParameter_ShouldThrow_HttpListenerException()
        {
            string missingIdParameter = new Error("Missing id parameter").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Post, "key");
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingIdParameter))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithoutBody_ShouldThrow_HttpListenerException()
        {
            string missingKey = new Error("Missing key").ToString();

            context = await listener.GenerateContext("?id=test", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingKey))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithInvalidKey_ShouldThrow_HttpListenerException()
        {
            string invalidKeyStructure = new Error("Invalid key structure").ToString();

            context = await listener.GenerateContext("?id=test", HttpMethod.Post, "invalidkey");
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidKeyStructure))
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
        public async void WhenUnhandledMethodRequest_ShouldThrow_HttpListenerException()
        {
            string notFound = new Error("Not Found").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Get);
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
