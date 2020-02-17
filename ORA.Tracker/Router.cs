using System;
using System.Net;
using System.Collections.Generic;

using ORA.Tracker.Routes;
using ORA.Tracker.Models;

namespace ORA.Tracker
{
    public class Router
    {
        private Dictionary<string, Route> routes;

        public Router()
        {
            this.routes = new Dictionary<string, Route>();
        }

        public void RegisterRoute(Route route) => this.routes.Add(route.Path, route);

        public void HandleRequest(HttpListenerContext context)
        {
            string path = context.Request.RawUrl;
            string body = new String("");

            if (this.isRouteHandled(path))
            {
                try
                {
                    body = this.routes[path].HandleRequest(context.Request, context.Response);
                }
                catch (HttpListenerException e)
                {
                    context.Response.StatusCode = e.ErrorCode;
                    body = e.Message;
                    // TODO: log this
                }
                catch (System.Exception e)
                {
                    context.Response.StatusCode = 520;
                    body = Error.UnknownError;
                    // TODO: log this
                    Console.WriteLine(e.Message);
                }
            }
            else
            {
                context.Response.StatusCode = 404;
                body = Error.NotFound;
                // TODO: Log this
            }

            this.sendResponse(body, context.Response);
        }

        private bool isRouteHandled(string path) => this.routes.ContainsKey(path);

        private bool sendResponse(string responseContent, HttpListenerResponse response)
        {
            byte[] buffer = System.Text.Encoding.UTF8.GetBytes(responseContent);

            response.ContentLength64 = buffer.Length;
            System.IO.Stream output = response.OutputStream;

            try
            {
                output.Write(buffer, 0, buffer.Length);
                output.Close();
            }
            catch (ObjectDisposedException e)
            {
                // TODO: Log this
                Console.WriteLine(e.Message);
                return false;
            }
            catch (HttpListenerException e)
            {
                // TODO: Log this
                Console.WriteLine(e.Message);
                return false;
            }

            return true;
        }
    }
}
