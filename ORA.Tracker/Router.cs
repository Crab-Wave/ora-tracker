using System;
using System.Net;
using System.Net.Http;
using System.Collections.Generic;
using System.IO;

using ORA.Tracker.Routes;
using ORA.Tracker.Models;
using ORA.Tracker.Logging;

namespace ORA.Tracker
{
    public class Router
    {
        private static readonly Logger logger = new Logger();

        private Dictionary<string, Route> routes;

        public Router()
        {
            this.routes = new Dictionary<string, Route>();
        }

        public void RegisterRoute(Route route) => this.routes.Add(route.Path, route);

        public void HandleRequest(HttpListenerContext context)
        {
            string path = "/" + (context.Request.RawUrl.Split("?")[0].Split("/")[1] ?? "");
            byte[] body = new byte[0];

            if (this.isRouteHandled(path))
            {
                try
                {
                    body = this.routes[path].HandleRequest(context.Request, context.Response);
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
                    body = Error.UnknownErrorBytes;
                }
            }
            else
            {
                logger.Error("Unhandled route");

                context.Response.StatusCode = 404;
                if (context.Request.HttpMethod != HttpMethod.Head.ToString())
                    body = Error.NotFoundBytes;
            }

            this.sendResponse(body, context.Response);
        }

        private bool isRouteHandled(string path) => this.routes.ContainsKey(path);

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
