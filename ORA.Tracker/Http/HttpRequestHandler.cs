using System;
using System.Net;

namespace ORA.Tracker.Http
{
    public class HttpRequestHandler
    {
        private Action<HttpRequest, HttpListenerResponse, HttpRequestHandler> handler;
        private HttpRequestHandler next;

        public HttpRequestHandler Next
        {
            get => this.next;
            set => this.next = value;
        }

        public HttpRequestHandler(Action<HttpRequest, HttpListenerResponse, HttpRequestHandler> handler,
            HttpRequestHandler next)
        {
            this.handler = handler;
            this.next = next;
        }

        public void Handle(HttpRequest request, HttpListenerResponse response) =>
            this.handler(request, response, next);
    }
}
