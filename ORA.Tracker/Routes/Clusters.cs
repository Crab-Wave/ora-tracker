using System;
using System.Net;

namespace ORA.Tracker.Routes
{
    public class Clusters : Route
    {
        public Clusters()
            : base("/clusters") { }

        protected override byte[] get(HttpListenerRequest request, HttpListenerResponse response)
        {
            return new byte[0];
        }

        // create cluster return id as json
        protected override byte[] post(HttpListenerRequest request, HttpListenerResponse response)
        {
            Guid id = new Guid();
            return new byte[0];
        }

        protected override byte[] delete(HttpListenerRequest request, HttpListenerResponse response)
        {
            return new byte[0];
        }
    }
}
