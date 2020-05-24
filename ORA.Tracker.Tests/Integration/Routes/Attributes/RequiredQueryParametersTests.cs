using System.Net;
using System.Net.Http;
using System.Text;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Tests.Integration.Utils;
using ORA.Tracker.Http;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes.Attributes.Tests.Integration
{
    public class RequiredQueryParametersTests
    {
        private static readonly MockupRouter router = new MockupRouter("/", new MockupRoute());

        [Fact]
        public async void Request_WhenMissingQueryParameter_ShouldRespondWithBadRequest()
        {
            string expectedResponseContent = new Error("Missing query parameter id").ToString();

            var response = await router.GetResponseOf(HttpMethod.Get, "/");

            response.StatusCode.Should().Be(400);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        [Fact]
        public async void Request_WhenQueryParameter_ShouldRespondWithOk()
        {
            string expectedResponseContent = "The request is correct";

            var response = await router.GetResponseOf(HttpMethod.Get, "?id=id");

            response.StatusCode.Should().Be(200);
            response.Content.ReadAsStringAsync().Result.Should().Be(expectedResponseContent);
        }

        private class MockupRoute : Route
        {
            public MockupRoute()
                : base(null) { }

            [RequiredQueryParameters("id")]
            protected override void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
                => response.Close(Encoding.UTF8.GetBytes("The request is correct"), true);
        }
    }
}
