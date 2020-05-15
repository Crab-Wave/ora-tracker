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
        private static readonly MockupRouter router = new MockupRouter("/auth", new Authentication(new ServiceCollection(null)));

        [Fact]
        public async void WhenPostWithoutBody_ShouldRespondWithBadRequest()
        {
            string missingKey = new Error("Missing key").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/auth");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(missingKey);
        }

        [Fact]
        public async void WhenPostWithTooSmallKey_ShouldRespondWithBadRequest()
        {
            string invalidKeyStructure = new Error("Invalid key structure").ToString();
            var key = Convert.ToBase64String(new byte[15] { 1, 1, 2, 3, 5, 8, 13, 21, 34, 55, 89, 114, 233, 1, 1 });

            var response = await router.GetResponseOf(HttpMethod.Post, "/auth",
                Encoding.UTF8.GetBytes(key));

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(invalidKeyStructure);
        }

        [Fact]
        public async void WhenPostWithInvalidKey_ShouldRespondWithBadRequest()
        {
            string invalidKeyStructure = new Error("Invalid key structure").ToString();

            var response = await router.GetResponseOf(HttpMethod.Post, "/auth",
                Encoding.UTF8.GetBytes(Convert.ToBase64String(Encoding.UTF8.GetBytes("invalidkeyinvalidkey"))));

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(invalidKeyStructure);
        }

        [Fact]
        public async void WhenPostWithValidKey_ShouldRespondWithToken()
        {
            var csp = new RSACryptoServiceProvider();
            byte[] publicKey = csp.ExportRSAPublicKey();

            var response = await router.GetResponseOf(HttpMethod.Post, "/auth",
                Encoding.UTF8.GetBytes(Convert.ToBase64String(publicKey)));

            // TODO: write test
        }

        [Fact]
        public async void WhenHeadRequest_ShouldRespondWithNotFoundAndEmptyBody()
        {
            var response = await router.GetResponseOf(HttpMethod.Head, "/auth");

            response.StatusCode.Should().Be(404);
            response.Content.ReadAsByteArrayAsync().Result.Should().BeEmpty();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldRespondWithNotFound()
        {
            string notFound = new Error("Not Found").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/auth");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Put, "/auth");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Delete, "/auth");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);

            response = await router.GetResponseOf(HttpMethod.Options, "/auth");
            response.StatusCode.Should().Be(404);
            response.Content.ReadAsStringAsync().Result.Should().Be(notFound);
        }
    }
}
