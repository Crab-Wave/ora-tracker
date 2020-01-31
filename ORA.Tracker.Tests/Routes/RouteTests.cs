using System.Net;
using System.Net.Http;
using Xunit;
using FluentAssertions;

using ORA.Tracker.Tests.Utils;
using ORA.Tracker.Routes;

namespace ORA.Tracker.Routes.Tests
{
    public class RouteTests
    {
        [Fact]
        public async void WhenUnhandledMethod_ShouldReturnFalse()
        {
            var testee = new MockRoute();

            HttpListenerContext context;

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Get);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Head);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Post);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Put);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Delete);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Options);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Trace);
            testee.HandleRequest(context.Request, context.Response).Should().BeFalse();
        }
    }

    internal class MockRoute : Route
    {
        public MockRoute()
            : base("/route") { }
    }
}
