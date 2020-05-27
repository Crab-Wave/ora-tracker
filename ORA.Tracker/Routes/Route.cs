using System;
using System.Net;
using System.Collections.Generic;
using System.Reflection;
using System.Linq;

using ORA.Tracker.Http;
using ORA.Tracker.Routes.Attributes;
using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public abstract class Route
    {
        private static byte[] methodNotAllowed = new Error("Method Not Allowed").ToBytes();
        private static byte[] notFound = new Error("Not Found").ToBytes();

        protected IServiceCollection services { get; }
        private Dictionary<string, HttpRequestHandler> handlers;

        public Route(IServiceCollection services)
        {
            this.services = services;

            this.handlers = new Dictionary<string, HttpRequestHandler>
            {
                { "GET", buildHandler("get") },
                { "HEAD", buildHandler("head") },
                { "POST", buildHandler("post") },
                { "PUT", buildHandler("put") },
                { "DELETE", buildHandler("delete") },
                { "OPTIONS", buildHandler("options") }
            };
        }

        public void HandleRequest(HttpRequest request, HttpListenerResponse response)
        {
            if (this.handlers.ContainsKey(request.HttpMethod))
                this.handlers[request.HttpMethod].Handle(request, response);
            else
                response.MethodNotAllowed(methodNotAllowed);
        }

        private HttpRequestHandler buildHandler(string httpMethod)
        {
            MethodInfo method = this.GetType().GetMethod(httpMethod,
                BindingFlags.Instance | BindingFlags.NonPublic);

            var handlers = new List<HttpRequestHandler>();

            foreach (Object attributes in method.GetCustomAttributes(false))
            {
                if (attributes is RequiredQueryParameters)
                    handlers.Add(new HttpRequestHandler((attributes as RequiredQueryParameters).GetHandler(), null));
                else if (attributes is RequiredUrlParameters)
                    handlers.Add(new HttpRequestHandler((attributes as RequiredUrlParameters).GetHandler(), null));
                else if (attributes is Authenticate)
                    handlers.Add(new HttpRequestHandler((attributes as Authenticate).GetHandler(this.services), null));

                if (handlers.Count > 1)
                    handlers[handlers.Count-2].Next = handlers.Last();
            }

            handlers.Add(new HttpRequestHandler((request, response, next) =>
                this.GetType().GetMethod(httpMethod, BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(this, new object[] { request, response, next }), null));
            if (handlers.Count > 1)
                handlers[handlers.Count-2].Next = handlers.Last();

            return handlers[0];
        }

        protected virtual void get(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.NotFound(notFound);

        protected virtual void head(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.NotFound(new byte[] { });

        protected virtual void post(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.NotFound(notFound);

        protected virtual void put(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.NotFound(notFound);

        protected virtual void delete(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.NotFound(notFound);

        protected virtual void options(HttpRequest request, HttpListenerResponse response, HttpRequestHandler next)
            => response.NotFound(notFound);
    }
}
