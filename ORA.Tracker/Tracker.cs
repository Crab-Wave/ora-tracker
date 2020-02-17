using ORA.Tracker.Routes;

namespace ORA.Tracker
{
    public class Tracker
    {
        private readonly Server server;

        public Tracker(int port)
        {
            this.server = new Server(port);

            this.server.RegisterRoute(new Root());
        }

        public void Start() => this.server.Listen();

        public void Stop() => this.server.Stop();
    }
}
