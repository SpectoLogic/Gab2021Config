using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;

namespace appcfgdemo.Services
{
    public class ConfigurationUpdater : IConfigurationUpdater
    {
        private readonly ILogger<ConfigurationUpdater> _logger;
        public ConfigurationUpdater(ILogger<ConfigurationUpdater> logger)
        {
            _logger = logger;
        }
        public Task StartMonitoring(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start monitoring,... ");
            while (!cancellationToken.IsCancellationRequested)
            {
            }
            return Task.CompletedTask;
        }
        public Task StopMonitoring(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stop monitoring,... ");
            return Task.CompletedTask;
        }
    }
}
