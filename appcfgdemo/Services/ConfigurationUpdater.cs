using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace appcfgdemo.Services
{
    public class ConfigurationUpdater : IConfigurationUpdater
    {
        private readonly ILogger<ConfigurationUpdater> _logger;
        private readonly IConfigurationRefresher _refresher;

        private const string ServiceBusConnectionString = "Endpoint=sb://appcfgdemo.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<SECRETKEY>";
        private const string ServiceBusTopic = "cfgTopic";
        private const string ServiceBusSubscription = "cfgSubscription";
        private SubscriptionClient _serviceBusClient;

        public ConfigurationUpdater(
            ILogger<ConfigurationUpdater> logger,
            IConfigurationRefresher refresher)
        {
            _logger = logger;
            _refresher = refresher;
        }
        public Task StartMonitoring(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Start monitoring,... ");
            _serviceBusClient = new SubscriptionClient(ServiceBusConnectionString, ServiceBusTopic, ServiceBusSubscription);
            _serviceBusClient.RegisterMessageHandler(
                handler: (message, cancellationToken) =>
                {
                    string messageText = Encoding.UTF8.GetString(message.Body);
                    JsonElement messageData = JsonDocument.Parse(messageText).RootElement.GetProperty("data");
                    string key = messageData.GetProperty("key").GetString();
                    _logger.LogInformation($"Event received for Key = {key}");
                    _refresher.SetDirty(new TimeSpan(0, 0, 0)); // Default: 30 seconds!
                    return Task.CompletedTask;
                },
                exceptionReceivedHandler: (exceptionargs) =>
                {
                    _logger.LogError($"{exceptionargs.Exception}");
                    return Task.CompletedTask;
                });

            while (!cancellationToken.IsCancellationRequested)
            {
                // await _refresher.TryRefreshAsync(); // If you do not use the middleware use this! NOP if chaching has not expired!
            }
            return Task.CompletedTask;
        }
        public async Task StopMonitoring(CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Stop monitoring,... ");
            await _serviceBusClient.UnregisterMessageHandlerAsync(new TimeSpan(0, 0, 10));
        }
    }
}
