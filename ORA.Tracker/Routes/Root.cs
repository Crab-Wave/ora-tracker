using System.Net;

namespace ORA.Tracker.Routes
{
    public class Root : Route
    {
        public Root()
            : base("/") { }

        protected override string get(HttpListenerRequest request, HttpListenerResponse response)
            => "Hey welcome to '/'";
    }
}
