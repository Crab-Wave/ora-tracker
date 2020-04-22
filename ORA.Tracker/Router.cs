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
            string path = "/" + (context.Request.RawUrl.Split("?")[0].Split("/")[1] ?? "");
            byte[] body = new byte[0];
            bool routeHandled = false;

            try
            {
                foreach (var route in this.routes.Keys)
                {
                    if (this.isPathMatching(path, route))
                    {
                        routeHandled = true;
                        body = this.routes[route].HandleRequest(context.Request, context.Response);
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

        private bool isPathMatching(string path, string route)
        {
            int i = 0, j = 0;
            while (i < path.Length && j < route.Length)
                i++;
            return path == route;
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
