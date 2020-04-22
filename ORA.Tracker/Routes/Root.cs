using System.Net;
using System.Text;
using System.Collections.Generic;

namespace ORA.Tracker.Routes
{
    public class Root : Route
    {
        private static readonly byte[] welcomeMessage = Encoding.UTF8.GetBytes("Hey welcome to '/'");

        public Root()
            : base() { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response, Dictionary<string, string> urlParams = null)
            => welcomeMessage;
    }
}
