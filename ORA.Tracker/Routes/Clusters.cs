using System.Net;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        public Clusters()
            : base("/clusters") { }

        protected override string get(HttpListenerRequest request, HttpListenerResponse response)
        {
            return "";
        }

        protected override string post(HttpListenerRequest request, HttpListenerResponse response)
        {
            return "";
        }

        protected override string delete(HttpListenerRequest request, HttpListenerResponse response)
        {
            return "";
        }
    }
}
