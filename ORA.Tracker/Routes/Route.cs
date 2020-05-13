using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ORA.Tracker.Http;
using ORA.Tracker.Services;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public abstract class Route
    {
        private static string methodNotAllowed = new Error("Method Not Allowed").ToString();
        private static string notFound = new Error("Not Found").ToString();

        private Dictionary<string, Func<HttpRequest, HttpListenerResponse, byte[]>> callbacks;
        protected IServiceCollection services { get; }

        public Route(IServiceCollection services)
        {
            this.callbacks = new Dictionary<string, Func<HttpRequest, HttpListenerResponse, byte[]>>
            {
                { "GET", this.get },
                { "HEAD", this.head },
                { "POST", this.post },
                { "PUT", this.put },
                { "DELETE", this.delete },
                { "OPTIONS", this.options }
            };

            this.services = services;
        }

        public byte[] HandleRequest(HttpRequest request, HttpListenerResponse response)
        {
            string httpMethod = request.HttpMethod;

            if (this.callbacks.ContainsKey(httpMethod))
                return this.callbacks[httpMethod](request, response);

            throw new HttpListenerException(405, methodNotAllowed);
        }

        protected virtual byte[] get(HttpRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] head(HttpRequest request, HttpListenerResponse response)
            => new byte[0];      // Head should return empty body

        protected virtual byte[] post(HttpRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] put(HttpRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] delete(HttpRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] options(HttpRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, notFound);
    }
}
