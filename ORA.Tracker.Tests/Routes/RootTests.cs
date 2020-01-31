using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Tests.Utils;
using ORA.Tracker.Routes;

namespace ORA.Tracker.Routes.Tests
{
    public class RootTests
    {
        private static readonly HttpClient client = new HttpClient();

        [Fact]
        public async void WhenHandledMethodRequest_ShouldReturnTrue()
        {
            var testee = new Root();

            HttpListenerContext context = await Generator.GenerateListenerContext("/", HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response).Should().BeTrue();
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldReturnFalse()
        {
            var testee = new Root();

            HttpListenerContext context;

            context = await Generator.GenerateListenerContext("/", HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/", HttpMethod.Post);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/", HttpMethod.Put);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/", HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/", HttpMethod.Options);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/", HttpMethod.Trace);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();
        }
    }
}
