using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Models;
using ORA.Tracker.Tests.Integration.Utils;

namespace ORA.Tracker.Routes.Tests.Integration
{
    public class MembersTests
    {
        private static readonly MockupListener listener = new MockupListener(15306);

        private Members testee = new Members(null);

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

            var (request, response) = await listener.GetContext("/", HttpMethod.Put);
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
