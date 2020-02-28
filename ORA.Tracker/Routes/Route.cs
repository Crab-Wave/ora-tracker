using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public abstract class Route
    {
        private Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, byte[]>> callbacks;
        public string Path { get; }

        public Route(string path)
        {
            this.Path = path;
            this.callbacks = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, byte[]>>
            {
                { "GET", this.get },
                { "HEAD", this.head },
                { "POST", this.post },
                { "PUT", this.put },
                { "DELETE", this.delete },
                { "OPTIONS", this.options }
            };
        }

        public byte[] HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string httpMethod = request.HttpMethod;

            if (this.callbacks.ContainsKey(httpMethod))
                return this.callbacks[httpMethod](request, response);
            throw new HttpListenerException(405, Error.MethodNotAllowed);
        }

        protected virtual byte[] get(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFoundString);

        protected virtual byte[] head(HttpListenerRequest request, HttpListenerResponse response)
            => new byte[0];      // Head should return empty body

        protected virtual byte[] post(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFoundString);

        protected virtual byte[] put(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFoundString);

        protected virtual byte[] delete(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFoundString);

        protected virtual byte[] options(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFoundString);
    }
}
