using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using shared.Mocks;
using client;
using System.Net.Http;

namespace test_client
{
    public class TestStartup : client.Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940

        public TestStartup(IConfiguration configuration) : base(configuration) {}

        public override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.Remove(services.SingleOrDefault(d => d.ServiceType == typeof(IHttpClientFactory)));
            services.AddSingleton<IHttpClientFactory>(sp => MockHttpClientFactory.GetMockFactory().Object);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
    }
}
