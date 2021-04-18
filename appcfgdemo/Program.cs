using appcfgdemo.Services;
using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace appcfgdemo
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            IConfigurationRefresher _refresher = null;

            var builder =
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.ConfigureAppConfiguration((hostingContext, config) =>
                    {
                        IConfigurationRoot settings = config.Build();

                        if (hostingContext.HostingEnvironment.IsDevelopment())
                        {
                            string connection = settings.GetConnectionString("AppConfig");
                            config.AddAzureAppConfiguration(options =>
                            {
                                options
                                    .Connect(connection)
                                    .ConfigureRefresh(refresh =>
                                    {
                                        refresh.Register("TestApp:Settings:Sentinel",
                                                         true)
                                        .SetCacheExpiration(TimeSpan.FromDays(30)); // Reduce the poll frequency!
                                    }
                                    )
                                    .Select(KeyFilter.Any, LabelFilter.Null)
                                    .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                                    .UseFeatureFlags(featureFlagOptions =>
                                    {
                                        featureFlagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
                                    });
                                _refresher = options.GetRefresher();
                            }
                            );
                        }
                        else
                        {
                            ManagedIdentityCredential credentials = new ManagedIdentityCredential();
                            string appConfigUrl = settings.GetConnectionString("AppConfigUrl");
                            config.AddAzureAppConfiguration(options =>
                            {
                                options
                                    .Connect(new Uri(appConfigUrl), credentials)
                                    .ConfigureKeyVault(kv =>
                                    {
                                        kv.SetCredential(credentials);
                                    })
                                    .ConfigureRefresh(refresh =>
                                    {
                                        refresh.Register("TestApp:Settings:Sentinel",
                                                         true)
                                        .SetCacheExpiration(TimeSpan.FromDays(30)); // Reduce the poll frequency!
                                    }
                                    )
                                    .Select(KeyFilter.Any, LabelFilter.Null)
                                    .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                                    .UseFeatureFlags(featureFlagOptions =>
                                    {
                                        featureFlagOptions.CacheExpirationInterval = TimeSpan.FromMinutes(5);
                                    });
                                _refresher = options.GetRefresher();
                            }
                            );
                        }

                    }).UseStartup<Startup>();
                })
                .ConfigureServices(services => 
                    services.AddHostedService<ConfigurationUpdateService>()
                    .AddSingleton<IConfigurationRefresher>(_refresher));

            return builder;
        }
    }
}
