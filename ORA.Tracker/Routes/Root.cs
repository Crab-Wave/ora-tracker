using System.Net;

namespace ORA.Tracker.Routes
{
    public class Root : Route
    {
        public Root()
            : base("/") { }

        protected override bool get(HttpListenerRequest request, HttpListenerResponse response)
                                    => SendResponse("Hey welcome to '/'", response);
    }
}
