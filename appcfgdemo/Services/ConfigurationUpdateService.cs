using Microsoft.Extensions.Hosting;
using System.Threading;
using System.Threading.Tasks;

namespace appcfgdemo.Services
{
    public class ConfigurationUpdateService : IHostedService
    {
        private readonly IConfigurationUpdater _configUpdater;
        public ConfigurationUpdateService(IConfigurationUpdater configUpdater)
        {
            _configUpdater = configUpdater;
        }
        public async Task StartAsync(CancellationToken cancellationToken)
            => await _configUpdater.StartMonitoring(cancellationToken);
        public async Task StopAsync(CancellationToken cancellationToken)
            => await _configUpdater.StopMonitoring(cancellationToken);
    }
}
