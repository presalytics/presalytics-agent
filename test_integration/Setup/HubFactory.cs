using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using hub;

namespace test_integration.Setup
{
    public class HubFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            DotNetEnv.Env.Load("./Resources/.env.txt");
            string hubUrl = System.Environment.GetEnvironmentVariable("HUB_URL");
            builder.UseConfiguration(
                        new ConfigurationBuilder()
                            .AddEnvironmentVariables()
                            .Build()
                        );
            builder.UseStartup<hub.Startup>();
            builder.UseUrls(hubUrl);
            builder.ConfigureServices(services => {

            });
        }
    }
}