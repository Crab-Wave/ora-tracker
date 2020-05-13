using ORA.Tracker.Services.Managers;
using ORA.Tracker.Services.Databases;

namespace ORA.Tracker.Services
{
    public class ServiceCollection : IServiceCollection
    {
        private ClusterManager clusterManager;
        private TokenManager tokenManager;

        public ClusterManager ClusterManager { get => clusterManager; }
        public TokenManager TokenManager { get => tokenManager; }

        public ServiceCollection(IClusterDatabase clusterDatabase)
        {
            tokenManager = new TokenManager();
            clusterManager = new ClusterManager(clusterDatabase);
        }
    }
}
