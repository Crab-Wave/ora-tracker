using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;

using ORA.Tracker.Routes;
using ORA.Tracker.Models;
using ORA.Tracker.Services;
using ORA.Tracker.Http;

namespace ORA.Tracker
{
    public class Router
    {
        private static readonly byte[] unknownError = new Error("Unknown Error").ToBytes();
        private static readonly byte[] notFound = new Error("Not Found").ToBytes();

        private Logger logger;
        private Dictionary<string, Route> routes;

        public Router()
        {
            this.logger = new Logger();
            this.routes = new Dictionary<string, Route>();
        }

        public void RegisterRoute(string path, Route route) => this.routes.Add(path, route);

        public void HandleRequest(HttpListenerContext context)
        {
            var (route, request) = this.getHandlingContext(context.Request);
            if (route == null)
            {
                logger.Error($"Unhandled route: {context.Request.RawUrl.Split("?")[0]}");

                if (request.HttpMethod == HttpMethod.Head.ToString())
                    context.Response.NotFound(new byte[] { });
                else
                    context.Response.NotFound(notFound);
                return;
            }

            try
            {
                route.HandleRequest(request, context.Response);
            }
            catch (Exception e)
            {
                logger.Error($"Unknown Error:\n{e}");

                if (request.HttpMethod == HttpMethod.Head.ToString())
                    context.Response.UnknownError(new byte[] { });
                else
                    context.Response.UnknownError(unknownError);
            }
        }

        private (Route, HttpRequest) getHandlingContext(HttpListenerRequest request)
        {
            string path = request.RawUrl.Split("?")[0];
            var urlParameters = new Dictionary<string, string>();

            foreach (var route in this.routes.Keys)
            {
                if (this.matchRoute(path, route, out urlParameters))
                    return (this.routes[route], new HttpRequest(request, urlParameters));
            }

            return (null, new HttpRequest(request, null));
        }

        private bool matchRoute(string path, string route, out Dictionary<string, string> urlParams)
        {
            urlParams = new Dictionary<string, string>();
            string name, param;
            int i = 0, j = 0;

            while (i < path.Length && j < route.Length)
            {
                if (route[j] == '{')
                {
                    // Reads the name of the param and advance to next /
                    name = "";
                    j += 1;
                    while (j < route.Length && route[j] != '}')
                        name += route[j++];
                    j += 1;

                    // Reads the value of the param
                    param = "";
                    while (i < path.Length && path[i] != '/')
                        param += path[i++];

                    urlParams.Add(name, param);
                }

                if (i < path.Length && j < route.Length && path[i] != route[j])
                {
                    urlParams = null;
                    return false;
                }

                i += 1;
                j += 1;
            }

            if (i >= path.Length && (j < route.Length && route[j] == '{' || j+1 < route.Length && route[j+1] == '{'))
            {
                name = "";
                j += j == '{' ? 1 : 2;
                while (j < route.Length && route[j] != '}')
                    name += route[j++];
                j += 1;

                urlParams.Add(name, "");
            }

            if (!(i >= path.Length && j >= route.Length))
                urlParams = null;
            return i >= path.Length && j >= route.Length;
        }
    }
}
