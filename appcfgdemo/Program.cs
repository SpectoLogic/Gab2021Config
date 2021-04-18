using Azure.Identity;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.AzureAppConfiguration;
using Microsoft.Extensions.Hosting;
using System;

namespace appcfgdemo
{
    public class Program
    {
        public static void Main(string[] args) => CreateHostBuilder(args).Build().Run();

        public static IHostBuilder CreateHostBuilder(string[] args) =>
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
                                options
                                    .Connect(connection)
                                    .ConfigureRefresh(refresh =>
                                    {
                                        refresh.Register("TestApp:Settings:Sentinel",
                                                         true)
                                        .SetCacheExpiration(TimeSpan.FromSeconds(30));
                                    }
                                    )
                                    .Select(KeyFilter.Any, LabelFilter.Null)
                                    .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                            );
                        }
                        else
                        {
                            ManagedIdentityCredential credentials = new ManagedIdentityCredential();
                            string appConfigUrl = settings.GetConnectionString("AppConfigUrl");
                            config.AddAzureAppConfiguration(options =>
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
                                        .SetCacheExpiration(TimeSpan.FromSeconds(30));
                                    }
                                    )
                                    .Select(KeyFilter.Any, LabelFilter.Null)
                                    .Select(KeyFilter.Any, hostingContext.HostingEnvironment.EnvironmentName)
                            );
                        }

                    }).UseStartup<Startup>();
                });
    }
}
