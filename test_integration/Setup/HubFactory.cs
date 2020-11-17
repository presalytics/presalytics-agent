using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Net;
using Serilog;
using WebMotions.Fake.Authentication.JwtBearer;
using hub;
using hub.Services;
using shared;
using System.Linq;


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
            //builder.UseStartup<hub.Startup>();
            builder.UseUrls(hubUrl);
            builder.ConfigureServices(services => {
                var sp = services.BuildServiceProvider();
                services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory)));
                services.AddSingleton<IHttpClientFactory>(sp => MockHttpClientFactory.GetMockFactory().Object);
                services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer();
                
            });
        }
    }
}