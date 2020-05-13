using System;
using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;
using System.Security.Cryptography;
using System.Text;

using ORA.Tracker.Services;
using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class AuthenticationTests
    {
        private static readonly MockupListener listener = new MockupListener(15303);
        private static readonly ServiceCollection services = new ServiceCollection(null);

        private Authentication testee;

        public AuthenticationTests()
        {
            this.testee = new Authentication(services);
        }

        [Fact]
        public async void WhenPostWithoutBody_ShouldThrow_HttpListenerException()
        {
            string missingKey = new Error("Missing key").ToString();

            var (request, response) = await listener.GetContext("/", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(request, response))
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
            var (request, response) = await listener.GetContext("/", HttpMethod.Post, Encoding.UTF8.GetBytes(key));
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(invalidKeyStructure))
                .Where(e => e.ErrorCode.Equals(400));
        }

        [Fact]
        public async void WhenPostWithInvalidKey_ShouldThrow_HttpListenerException()
        {
            string invalidKeyStructure = new Error("Invalid key structure").ToString();

            var (request, response) = await listener.GetContext("/", HttpMethod.Post,
                Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes("invalidkeyinvalidkey"))));
            testee.Invoking(t => t.HandleRequest(request, response))
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

            var (request, response) = await listener.GetContext("/", HttpMethod.Post, Encoding.UTF8.GetBytes(Convert.ToBase64String(publicKey)));
            testee.HandleRequest(request, response);

            // TODO: write test
        }

        [Fact]
        public async void WhenHeadRequest_ShouldReturn_EmptyBody()
        {
            var (request, response) = await listener.GetContext("/", HttpMethod.Head);
            testee.HandleRequest(request, response)
                .Should()
                .Equals(new byte[0]);
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldThrow_HttpListenerException()
        {
            string notFound = new Error("Not Found").ToString();

            var (request, response) = await listener.GetContext("/", HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            (request, response) = await listener.GetContext("/", HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            (request, response) = await listener.GetContext("/", HttpMethod.Delete);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));

            (request, response) = await listener.GetContext("/", HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(request, response))
                .Should()
                .Throw<HttpListenerException>()
                .Where(e => e.Message.Equals(notFound))
                .Where(e => e.ErrorCode.Equals(404));
        }
    }
}
