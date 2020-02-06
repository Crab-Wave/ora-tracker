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
            testee.HandleRequest(context.Request, context.Response)
                .Should()
                .Be("Hey welcome to '/'");
        }

        [Fact]
        public async void WhenUnhandledMethodRequest_ShouldReturnFalse()
        {
            var testee = new Root();
            string notFound = "{\"message\":\"Not Found\",\"documentation_url\":\"https://ora.crabwave.com/documentation\"}";

            HttpListenerContext context;

            context = await Generator.GenerateListenerContext("/", HttpMethod.Head);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/", HttpMethod.Post);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/", HttpMethod.Put);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/", HttpMethod.Delete);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/", HttpMethod.Options);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);

            context = await Generator.GenerateListenerContext("/", HttpMethod.Trace);
            testee.Invoking(t => t.HandleRequest(context.Request, context.Response))
                .Should()
                .Throw<HttpListenerException>()
                .WithMessage(notFound);
        }
    }
}
