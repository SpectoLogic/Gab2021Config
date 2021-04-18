using System.Threading;
using System.Threading.Tasks;

namespace appcfgdemo.Services
{
    public interface IConfigurationUpdater
    {
        Task StartMonitoring(CancellationToken cancellationToken);
        Task StopMonitoring(CancellationToken cancellationToken);
    }
}
