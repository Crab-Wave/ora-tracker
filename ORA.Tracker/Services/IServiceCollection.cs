using ORA.Tracker.Services.Managers;

namespace ORA.Tracker.Services
{
    public interface IServiceCollection
    {
        ClusterManager ClusterManager { get; }
        TokenManager TokenManager { get; }
    }
}
