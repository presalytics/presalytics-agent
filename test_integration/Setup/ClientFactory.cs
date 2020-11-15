using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Mvc;
using client;
using DotNetEnv;

namespace test_integration.Setup
{
    public class ClientFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        
        protected override void ConfigureWebHost(IWebHostBuilder builder) {
            DotNetEnv.Env.Load("./Resources/.env.txt");
            string clientUrl = System.Environment.GetEnvironmentVariable("CLIENT_URL");
            builder.UseConfiguration(
                        new ConfigurationBuilder()
                            .AddEnvironmentVariables()
                            .Build()
                        );
            builder.UseStartup<client.Startup>();
            builder.UseUrls(clientUrl);
        }
    }
}