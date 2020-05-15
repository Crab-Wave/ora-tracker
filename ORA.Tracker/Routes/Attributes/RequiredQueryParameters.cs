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
                        handleMissingQueryParameter(p, response);
                        return;
                    }
                }

                next.Handle(request, response);
            };
        }

        private static void handleMissingQueryParameter(string parameter, HttpListenerResponse response)
        {
            response.StatusCode = 400;
            response.Close(new Error($"Missing query parameter {parameter}").ToBytes(), true);
        }
    }
}
