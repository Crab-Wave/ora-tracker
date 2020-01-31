using System;
using System.Net;
using System.Collections.Generic;

namespace ORA.Tracker.Routes
{
    public abstract class Route
    {
        private Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, bool>> callbacks;

        public string Path { get; }

        public Route(string path)
        {
            this.Path = path;
            this.callbacks = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, bool>>
            {
                { "GET", this.get },
                { "HEAD", this.head },
                { "POST", this.post },
                { "PUT", this.put },
                { "DELETE", this.delete },
                { "OPTIONS", this.options },
                { "TRACE", this.trace }
            };
        }

        public bool HandleRequest(HttpListenerRequest request, HttpListenerResponse response)
        {
            string httpMethod = request.HttpMethod;

            return this.callbacks.ContainsKey(httpMethod) ? this.callbacks[httpMethod](request, response) : false;
        }

        public static bool SendResponse(string responseContent, HttpListenerResponse response)
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

        protected virtual bool get(HttpListenerRequest request, HttpListenerResponse response) => false;

        protected virtual bool head(HttpListenerRequest request, HttpListenerResponse response) => false;

        protected virtual bool post(HttpListenerRequest request, HttpListenerResponse response) => false;

        protected virtual bool put(HttpListenerRequest request, HttpListenerResponse response) => false;

        protected virtual bool delete(HttpListenerRequest request, HttpListenerResponse response) => false;

        protected virtual bool options(HttpListenerRequest request, HttpListenerResponse response) => false;

        protected virtual bool trace(HttpListenerRequest request, HttpListenerResponse response) => false;
    }
}
