using System;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using cli.Commands;
using System.Threading.Tasks;
using CommandLine;
using System.Collections.Generic;
using System.IO;
using presalytics;
using presalytics.Services.Auth;
using System.Net.Http;

namespace cli
{
    class Program
    {
        static string Description = @"
********************************************************************************************

  Welcome to the Presalytics Command Line Interface (CLI)!

  With this tool, you can listen to and forward Presalytics events, 
  register a Presalytics agent, and issue admin API commands. 

  For more information, please visit our documentation at <https://presalytics.io/docs/>.

********************************************************************************************

";

        static void Main(string[] args)
        {
            Log.Logger = FileLogger.GetLogger();                  
            try
            {
                //Build IoC Container
                IServiceCollection services = new ServiceCollection();
                services.AddSingleton<ILogger>(Log.Logger);
                IServiceProvider sp = ConfigureServices(services);

                // Parse and Run commands
                Parser.Default.ParseArguments<LoginOptions, ListenOptions>(args)
                    .MapResult(
                        (LoginOptions opts) => sp.GetRequiredService<ICommandService<LoginOptions>>().Run(opts),
                        (ListenOptions opts) => sp.GetRequiredService<ICommandService<ListenOptions>>().Run(opts),
                        errs => HandleVerbError(errs)
                    );
                    
            } catch (Exception ex) {
                Log.Fatal(ex, "A Fatal Error occured");
                Environment.ExitCode = 1;
            } finally {
                Log.CloseAndFlush();
            }
        }

        static IServiceProvider ConfigureServices(IServiceCollection services)
        {
            Dictionary<string, string> defaultSettings = new Dictionary<string, string>{
                {"AUTH_DOMAIN", "login.presalytics.io"},
                {"CLIENT_ID", "znevv0xfhtHCGDYOXAPV6gSYrAGuJ6Fe"},
                {"EVENT_HUB_URL", "https://events.api.presalytics.io/hub"}
            };
            // Add Default Services
            var configuration = new ConfigurationBuilder()
                                    .AddInMemoryCollection(defaultSettings)
                                    .AddEnvironmentVariables()
                                    .Build();
            services.AddSingleton<IConfiguration>(configuration);
            services.AddHttpClient();


            // Add Presalytics Services
            services.AddPresaltyicsAccessTokenProvider(configuration);
            services.AddPresalyticsCommandLineLogin(configuration);
            services.AddPresaltyicsAccessTokenProvider(configuration);
            services.AddTransient<ITokenStore, LocalTokenStore>();
            services.AddPresalyticsEventsSocket(configuration);


            // Add Command Services
            services.AddSingleton<ICommandService<LoginOptions>, LoginCommandService>();
            services.AddSingleton<ICommandService<ListenOptions>, ListenCommandService>();


            // Build and Return Service Provider
            return services.BuildServiceProvider();
        }

        static int HandleVerbError(IEnumerable<Error> errors)
        {
            Console.Write(Description);
            foreach( var err in errors) {
                string message = string.Format("Invalid Input: {0}", err.Tag);
                if (err.Tag != ErrorType.HelpVerbRequestedError && err.Tag != ErrorType.HelpRequestedError) Console.WriteLine(message);
                Log.Error(message);
            }
            return 1;
        }
    }
}
