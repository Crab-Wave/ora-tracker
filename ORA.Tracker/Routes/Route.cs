using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public abstract class Route
    {
        private Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, string>> callbacks;
        public string Path { get; }

        public Route(string path)
        {
            this.Path = path;
            this.callbacks = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, string>>
            {
                { "GET", this.get },
                { "HEAD", this.head },
                { "POST", this.post },
                { "PUT", this.put },
                { "DELETE", this.delete },
                { "OPTIONS", this.options }
            };
        }

        public string HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string httpMethod = request.HttpMethod;

            if (this.callbacks.ContainsKey(httpMethod))
                return this.callbacks[httpMethod](request, response);
            throw new HttpListenerException(405, Error.MethodNotAllowed);
        }

        protected virtual string get(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFound);

        protected virtual string head(HttpListenerRequest request, HttpListenerResponse response)
            => "";      // Head should return empty body

        protected virtual string post(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFound);

        protected virtual string put(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFound);

        protected virtual string delete(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFound);

        protected virtual string options(HttpListenerRequest request, HttpListenerResponse response)
            => throw new HttpListenerException(404, Error.NotFound);
    }
}
