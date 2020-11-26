using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using WebMotions.Fake.Authentication.JwtBearer;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using System.Linq;
using System.Net.Http;
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
            builder.UseUrls(clientUrl);
            builder.ConfigureServices(services => {
                services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer();
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory)));
                services.AddSingleton<IHttpClientFactory>(sp => MockHttpClientFactory.GetMockFactory().Object);
            });
        }
    }
}