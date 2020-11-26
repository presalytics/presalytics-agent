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
using Microsoft.AspNetCore.SignalR;
using Serilog;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.AspNetCore.Authorization;
using System.IdentityModel.Tokens.Jwt;
using System.IdentityModel.Tokens;
using Microsoft.IdentityModel.Logging;
using Microsoft.AspNetCore.Cors;
using System.Net.Http;
using shared;
using shared.Auth;
using hub.Hubs;
using hub.Services;

namespace hub
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
            ILogger _logger = services.GetSerilogLogger();
            services.AddControllers();
            services.AddSignalR();

            JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
            
            string baseUrl = Configuration.GetValue<string>("AUTH0_DOMAIN");
        
            string issuer = string.Format("https://{0}/", baseUrl);
            string wellKnownEndpont = issuer + ".well-known/openid-configuration";

            _logger.Information("Auth Well Known Endpoint: " + wellKnownEndpont);

            var oidcConfigMgr = new ConfigurationManager<OpenIdConnectConfiguration>(
                wellKnownEndpont,
                new OpenIdConnectConfigurationRetriever(),
                new HttpDocumentRetriever()  // In production, tls termination is handled by istio, so the application will not see https endpoints.
            );

            OpenIdConnectConfiguration discoveryDocument = Task.Run(async () => await oidcConfigMgr.GetConfigurationAsync()).Result;
            List<SecurityKey> signingKeys = discoveryDocument.SigningKeys.ToList();
            
            services.AddSingleton<IConfigurationManager<OpenIdConnectConfiguration>>(oidcConfigMgr);
            
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options => 
                {
                    options.Authority = baseUrl;
                    options.MetadataAddress = wellKnownEndpont;
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidIssuer = issuer, // should be: https://auth.presalytics.io/auth/realms/presalytics
                        ValidateAudience = true,
                        ValidAudience = "https://api.presalytics.io/",
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKeys = signingKeys                
                    };
                    options.IncludeErrorDetails = true;
                });
        
            services.AddAuthorization(options => {
                options.AddPolicy("ViewPermission", policy => {
                    policy.Requirements.Add(new HasPermissionRequirement("view", issuer));
                });
                options.AddPolicy("BuildPermission", policy => {
                    policy.Requirements.Add(new HasPermissionRequirement("build", issuer));
                });
            });

            services.AddTransient<IAgentManager, AgentManager>();
            services.AddHttpClient("website", options => {
                options.BaseAddress = new Uri(Configuration.GetValue<string>("WEBSITE_URL"));
            });

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapHub<AgentHub>("/agent-hub");
            });
        }
    }
}
