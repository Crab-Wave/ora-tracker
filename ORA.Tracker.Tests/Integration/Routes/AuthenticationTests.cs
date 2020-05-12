using System;
using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;
using System.Security.Cryptography;
using System.Text;
using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Tests.Integration.Routes
{
    public class AuthenticationTests
    {
        private static readonly MockupListener listener = new MockupListener(15303);

        private Authentication testee = new Authentication();
        private HttpListenerContext context;

        [Fact]
        public async void WhenPostWithoutBody_ShouldThrow_HttpListenerException()
        {
            string missingKey = new Error("Missing key").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(missingKey))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithTooSmallKey_ShouldThrow_HttpListenerException()
        {
            string invalidKeyStructure = new Error("Invalid key structure").ToString();

            var key = Convert.ToBase64String(new byte[15] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 114, 233, 1, 1 });
            context = await listener.GenerateContext("/", HttpMethod.Post, Encoding.UTF8.GetBytes(key));
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidKeyStructure))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithInvalidKey_ShouldThrow_HttpListenerException()
        {
            string invalidKeyStructure = new Error("Invalid key structure").ToString();

            context = await listener.GenerateContext("/", HttpMethod.Post,
                Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes("invalidkeyinvalidkey"))));
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidKeyStructure))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithValidKey_ShouldReturn_Token()
        {
            var csp = new RSACryptoServiceProvider();
            byte[] publicKey = csp.ExportRSAPublicKey();

            context = await listener.GenerateContext("/", HttpMethod.Post, Encoding.UTF8.GetBytes(Convert.ToBase64String(publicKey)));
            testee.HandleRequest(context.Request, context.Response);

            // TODO: write test
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
