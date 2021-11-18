namespace cli.Commands
{
    using CommandLine;
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using presalytics.Services.Auth;
    using presalytics.Models;

    [Verb("login", HelpText = "Login to Preaslytics API.")]
    public class LoginOptions : CommandOptionsBase {}

    public class LoginCommandService : CommandServiceBase<LoginOptions>, ICommandService<LoginOptions>
    {
        public LoginCommandService(ILogger logger, ILogin login, ITokenStore tokenStore) : base(logger, login, tokenStore) {}

        protected override async Task DoWork()
        {
            await Login(); 
        }
    }
}