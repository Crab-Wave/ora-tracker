using System;
using System.Net;

using ORA.Tracker.Routes;
using ORA.Tracker.Services;

namespace ORA.Tracker
{
    public class Server
    {
        private Logger logger;
        private HttpListener listener;
        private readonly Router router;
        private int port;

        public Server(int port)
        {
            this.logger = new Logger();
            this.port = port;
            this.listener = new HttpListener();
            this.listener.Prefixes.Add($"http://localhost:{port.ToString()}/");
            this.listener.Prefixes.Add($"http://+:{port.ToString()}/");

            this.router = new Router();
        }

        public void Listen()
        {
            this.listener.Start();
            logger.Info($"Server started, now listening on port {this.port}...");

            while (this.listener.IsListening)
                this.tryGetContext();
        }

        public void RegisterRoute(string path, Route route) => this.router.RegisterRoute(path, route);

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

            this.router.HandleRequest(this.listener.EndGetContext(result));
        }
    }
}
