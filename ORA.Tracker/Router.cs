using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;

using ORA.Tracker.Routes;
using ORA.Tracker.Models;
using ORA.Tracker.Services;

namespace ORA.Tracker
{
    public class Router
    {
        private static readonly Logger logger = new Logger();
        private static readonly byte[] unknownError = new Error("Unknown Error").ToBytes();
        private static readonly byte[] notFound = new Error("Not Found").ToBytes();

        private Dictionary<string, Route> routes;

        public Router()
        {
            this.routes = new Dictionary<string, Route>();
        }

        public void RegisterRoute(string path, Route route) => this.routes.Add(path, route);

        public void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.RawUrl.Split("?")[0];
            Dictionary<string, string> urlParams;
            var body = new byte[0];
            bool routeHandled = false;

            try
            {
                foreach (var route in this.routes.Keys)
                {
                    if (this.matchRoute(path, route, out urlParams))
                    {
                        routeHandled = true;
                        body = this.routes[route].HandleRequest(context.Request, context.Response, urlParams);
                        break;
                    }
                }

            }
            catch (HttpListenerException e)
            {
                logger.Error(e);

                context.Response.StatusCode = e.ErrorCode;
                body = System.Text.Encoding.UTF8.GetBytes(e.Message);
            }
            catch (Exception e)
            {
                logger.Error(e);

                context.Response.StatusCode = 520;
                body = unknownError;
            }

            if (!routeHandled)
            {
                logger.Error("Unhandled route");

                context.Response.StatusCode = 404;
                if (context.Request.HttpMethod != HttpMethod.Head.ToString())
                    body = notFound;
            }

            this.sendResponse(body, context.Response);
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

        private bool sendResponse(byte[] buffer, HttpListenerResponse response)
        {
            response.ContentLength64 = buffer.Length;
            Stream output = response.OutputStream;

            try
            {
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (Exception e)
            {
                logger.Error(e);
                return false;
            }

            return true;
        }
    }
}
