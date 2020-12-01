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
using hub;
using shared.Mocks;
using System.Net.Http;

namespace test_hub
{
    public class TestStartup : hub.Startup
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
    }
}
