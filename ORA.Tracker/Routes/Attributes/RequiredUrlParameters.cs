using System;
using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequiredUrlParameters : Attribute
    {
        private string[] parameters;
        public string[] Parameters { get => this.parameters; }

        public RequiredUrlParameters(params string[] parameters)
        {
            this.parameters = parameters;
        }

        public Action<HttpRequest, HttpListenerResponse, HttpRequestHandler> GetHandler()
        {
            return (request, response, next) =>
            {
                foreach (var p in this.Parameters)
                {
                    if (request.UrlParameters == null || !request.UrlParameters.ContainsKey(p) || request.UrlParameters[p] == "")
                    {
                        response.BadRequest(new Error($"Missing url parameter {p}").ToBytes());
                        return;
                    }
                }

                next.Handle(request, response);
            };
        }
    }
}
