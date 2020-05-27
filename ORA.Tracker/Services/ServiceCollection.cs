using ORA.Tracker.Services.Managers;
using ORA.Tracker.Services.Databases;

namespace ORA.Tracker.Services
{
    public class ServiceCollection : IServiceCollection
    {
        private ClusterManager clusterManager;
        private TokenManager tokenManager;
        private NodeManager nodeManager;

        public ClusterManager ClusterManager { get => clusterManager; }
        public TokenManager TokenManager { get => tokenManager; }
        public NodeManager NodeManager { get => nodeManager; }

        public ServiceCollection(IDatabase clusterDatabase)
        {
            this.tokenManager = new TokenManager();
            this.clusterManager = new ClusterManager(clusterDatabase);
            this.nodeManager = new NodeManager();
        }
    }
}
