using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public class Router
    {
        private static readonly string notFound = new Error("Not Found").ToString();
        private Dictionary<string, Route> routes;

        public Router()
        {
            this.routes = new Dictionary<string, Route>();
        }

        public void RegisterRoute(Route route) => this.routes.Add(route.Path, route);

        public void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.RawUrl;

            if (!this.isRouteHandled(path)
             || !this.routes[path].HandleRequest(context.Request, context.Response))
            {
                // TODO: Log this

                Route.SendResponse(notFound, context.Response);
            }
        }

        private bool isRouteHandled(string path) => this.routes.ContainsKey(path);
    }
}
