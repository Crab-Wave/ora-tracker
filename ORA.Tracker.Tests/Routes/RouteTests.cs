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
        public async void WhenUnhandledMethod_Throws_HttpListenerException()
        {
            var testee = new MockRoute();
            string notFound = "{\r\n  \"message\": \"Not Found\",\r\n  \"documentation_url\": \"https://ora.crabwave.com/documentation\"\r\n}";

            HttpListenerContext context;

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Get);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Head);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Delete);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/route", HttpMethod.Trace);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);
        }
    }

    internal class MockRoute : Route
    {
        public MockRoute()
            : base("/route") { }
    }
}
