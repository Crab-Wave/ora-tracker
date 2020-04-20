using System;
using System.Net;

namespace ORA.Tracker.Routes
{
    public class Token : Route
    {
        public Token()
            : base() { }

        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            string[] urlParams = this.getUrlParams(request);

            return new Models.Token(120, "hey id").Serialize();
        }
    }
}
