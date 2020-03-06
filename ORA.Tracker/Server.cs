using System;
using System.Net;

using ORA.Tracker.Routes;
using ORA.Tracker.Logging;

namespace ORA.Tracker
{
    public class Server
    {
        private static readonly Logger logger = new Logger();
        private HttpListener listener;
        private readonly Router router;

        public Server(int port)
        {
            this.listener = new HttpListener();
            this.listener.Prefixes.Add($"http://localhost:{port.ToString()}/");

            this.router = new Router();
        }

        public void Listen()
        {
            this.listener.Start();

            while (this.listener.IsListening)
                this.tryGetContext();
        }

        public void RegisterRoute(Route route) => this.router.RegisterRoute(route);

        public void Stop() => this.listener.Stop();

        private void tryGetContext()
        {
            try
            {
                IAsyncResult result = this.listener.BeginGetContext(new AsyncCallback(this.listenerCallback), this.listener);
                result.AsyncWaitHandle.WaitOne();
            }
            catch (Exception e)
            {
                logger.Error(e);
            }
        }

        private void listenerCallback(IAsyncResult result)
        {
            this.listener = (HttpListener) result.AsyncState;

            HttpListenerContext context = this.listener.EndGetContext(result);
            this.router.HandleRequest(context);
        }
    }
}
