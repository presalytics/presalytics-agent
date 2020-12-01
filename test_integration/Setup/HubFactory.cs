using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Options;
using System.Net.Http;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Net;
using Serilog;
using WebMotions.Fake.Authentication.JwtBearer;
using hub;
using hub.Services;
using shared;
using System.Linq;
using System.IO;
using System.Threading.Tasks;
using test_integration.Services;


namespace test_integration.Setup
{
    // public class HubFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    // {
    //     protected override void ConfigureWebHost(IWebHostBuilder builder) {
    //         DotNetEnv.Env.Load("./Resources/.env.txt");
    //         string hubUrl = System.Environment.GetEnvironmentVariable("HUB_URL");
    //         builder.UseConfiguration(
    //                     new ConfigurationBuilder()
    //                         .AddEnvironmentVariables()
    //                         .Build()
    //                     );
    //         //builder.UseStartup<hub.Startup>();
    //         builder.UseUrls(hubUrl);
    //         builder.ConfigureServices(services => {
    //             var sp = services.BuildServiceProvider();
    //             services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory)));
    //             services.AddSingleton<IHttpClientFactory>(sp => MockHttpClientFactory.GetMockFactory().Object);
    //             services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer();
    //         });
    //     }
    // }

    public class TestHubContainer
    {

        public IWebHost Host { get; set;}

        public TestHubContainer()
        {
            DotNetEnv.Env.Load("./Resources/.env.txt");
            string hubUrl = System.Environment.GetEnvironmentVariable("HUB_URL");
            Host = new WebHostBuilder()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .UseConfiguration(new ConfigurationBuilder()
                    .AddEnvironmentVariables()
                    .Build()
                )
                .UseStartup<TestStartup>()
                .UseKestrel()
                .UseUrls(hubUrl)
                .Build();
        }
        public void Start()
        {
            Host.Start();
        }

        public string GetHostUrl()
        {
            return Host.ServerFeatures.Get<IServerAddressesFeature>().Addresses.Single();
        }

        public async Task<string> GetReport()
        {
            return await Host.Services.GetRequiredService<IEventReportSink>().Report.Task;
        }
    }

    public class TestStartup : hub.Startup
    {
        public TestStartup(IConfiguration configuration) : base(configuration) {}

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory)));
            services.AddSingleton<IHttpClientFactory>(sp => MockHttpClientFactory.GetMockFactory().Object);
            services.AddAuthentication(FakeJwtBearerDefaults.AuthenticationScheme).AddFakeJwtBearer();
            services.AddSingleton<IEventReportSink, EventReportSink>();
            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IAgentManager)));
            services.AddTransient<IAgentManager, TestAgentManager>();

        }
    }
}