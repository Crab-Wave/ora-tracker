using System;
using System.Net;
using System.Collections.Generic;
using System.IO;
using System.Text;

using ORA.Tracker.Models;

namespace ORA.Tracker.Routes
{
    public abstract class Route
    {
        private static string methodNotAllowed = new Error("Method Not Allowed").ToString();
        private static string notFound = new Error("Not Found").ToString();

        private Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>, byte[]>> callbacks;

        public Route()
        {
            this.callbacks = new Dictionary<string, Func<HttpListenerRequest, HttpListenerResponse, Dictionary<string, string>, byte[]>>
            {
                { "GET", this.get },
                { "HEAD", this.head },
                { "POST", this.post },
                { "PUT", this.put },
                { "DELETE", this.delete },
                { "OPTIONS", this.options }
            };
        }

        public byte[] HandleRequest(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams = null)
        {
            string httpMethod = request.HttpMethod;

            if (this.callbacks.ContainsKey(httpMethod))
                return this.callbacks[httpMethod](request, response, urlParams);

            throw new HttpListenerException(405, methodNotAllowed);
        }

        protected virtual byte[] get(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] head(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
            => new byte[0];      // Head should return empty body

        protected virtual byte[] post(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] put(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] delete(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
            => throw new HttpListenerException(404, notFound);

        protected virtual byte[] options(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams)
            => throw new HttpListenerException(404, notFound);

        protected string[] getUrlParams(HttpListenerRequest request)
        {
            string[] p = request.RawUrl.Split("?")[0].Split("/");
            string[] urlParams = new string[p.Length-2];

            for (int i = 0; i < urlParams.Length; i++)
                urlParams[i] = p[i+2];

            return urlParams;
        }

        protected byte[] getBody(HttpListenerRequest request)
        {
            Stream body = request.InputStream;
            Encoding bodyEncoding = request.ContentEncoding;
            var ms = new MemoryStream(512);
            body.CopyTo(ms);

            return ms.ToArray();
        }
    }
}
