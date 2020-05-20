using System;
using System.Net;

using ORA.Tracker.Http;
using ORA.Tracker.Models;

namespace ORA.Tracker.Routes.Attributes
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RequiredQueryParameters : Attribute
    {
        private string[] parameters;
        public string[] Parameters { get => this.parameters; }

        public RequiredQueryParameters(params string[] parameters)
        {
            this.parameters = parameters;
        }

        public Action<HttpRequest, HttpListenerResponse, HttpRequestHandler> GetHandler()
        {
            return (request, response, next) =>
            {
                foreach (var p in this.Parameters)
                {
                    if (request.QueryString.GetValues(p) == null)
                    {
                        response.BadRequest(new Error($"Missing query parameter {p}").ToBytes());
                        return;
                    }
                }

                next.Handle(request, response);
            };
        }
    }
}
