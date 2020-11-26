using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Authentication;
using client.SignalRClient;
using client.Services;
using shared;


namespace client
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var logConfig = SerilogLogger.GetLoggerConfiguration(Configuration);
            services.UseSerilogLogger(logConfig);

            
            services.AddControllers();

            services.AddHttpClient("workspace", c => {
                c.BaseAddress = new Uri(Configuration.GetValue<string>("WORKSPACE_URL"));
            });
            

            services.AddHttpContextAccessor();
            services.AddSingleton<ITokenProvider, TokenProvider>();

            services.AddSingleton<ISignalRClientMaster>(sp => 
            {
                var conn = new SignalRClientMaster(sp);
                
                conn.MessageReceived += async (object sender, EventArgs e) => {
                    ISocketBroker broker = sp.GetRequiredService<ISocketBroker>();
                    await broker.HandleMessageReceivedEvent(sender, e);
                };
                conn.SetHubUrl(Configuration.GetValue<string>("SIGNALR_HUB_URL"));
                conn.Connect().GetAwaiter().GetResult();
                
                return conn;
            });

            services.AddTransient<IWorkspace, Workspace>();
            services.AddTransient<ISocketBroker, SocketBroker>();

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
