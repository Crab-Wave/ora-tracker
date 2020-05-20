using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Services;
using ORA.Tracker.Http;
using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Attributes.Tests.Integration
{
    public class AuthenticateTests
    {
        private static readonly ServiceCollection services;
        private static readonly MockupRouter router;

        static AuthenticateTests()
        {
            services = new ServiceCollection(null);
            router = new MockupRouter("/", new MockupRoute(services));
        }

        [Fact]
        public async void Request_WhenMissingCredentials_ShouldRespondWithBadResponse()
        {
            string expectedResponseContent = new Error("Missing credentials").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Request_WhenInvalidCredentialsType_ShouldRespondWithBadRequest()
        {
            string invalidCredentialsType = "NotBearer";
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(Guid.NewGuid().ToString(), token);
            string expectedResponseContent = new Error("Invalid credentials type").ToString();

            var request = new MockupRouterRequest(HttpMethod.Get, "/")
            {
                CredentialsType = invalidCredentialsType,
                Credentials = token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Request_WhenInvalidToken_ShouldRespondWithBadResponse()
        {
            string invalidToken = "thisisaninvalidtoken";
            string expectedResponseContent = new Error("Invalid token").ToString();

            var request = new MockupRouterRequest(HttpMethod.Get, "/")
            {
                Credentials = invalidToken
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Request_WhenValidToken_ShouldRespondWithOk()
        {
            string token = services.TokenManager.NewToken();
            services.TokenManager.RegisterToken(Guid.NewGuid().ToString(), token);
            string expectedResponseContent = "You are authorized to acces this.";

            var request = new MockupRouterRequest(HttpMethod.Get, "/")
            {
                Credentials = token
            };
            var response = await router.GetResponseOf(request);

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }
    }

    internal class MockupRoute : Route
    {
        public MockupRoute(IServiceCollection services)
            : base(services) { }

        [Authenticate]
        protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.Close(Encoding.UTF8.GetBytes("You are authorized to acces this."), true);
    }
}
