namespace cli.Commands
{
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using presalytics.Services.Auth;
    using presalytics.Models;
    using CommandLine;

    public abstract class CommandOptionsBase
    {
        [Option('v', "verbose", Required = false, Default = false, HelpText = "Increase desciptiveness of console output")]
        public bool Verbose { get; set;}

        [Option('s', "silent", Required = false, Default = false, HelpText = "Supress console output.")]
        public bool Silent { get; set;}

        [Option("no-cache-credentials", Required = false, Default = false, HelpText = "Bypass cached login credentials and re-login")]
        public bool NoCache { get; set;}

        [Option("scope", Required = false, Default = "\'email profile offline_acccess\'", HelpText = "The user login scope." )]
        public string Scope {get ; set;}
    }

    public interface ICommandService<T> where T : CommandOptionsBase
    {
        int Run(T options);
    }

    public abstract class CommandServiceBase<T> : ICommandService<T>
        where T : CommandOptionsBase
    {
        protected CommandServiceBase(ILogger logger, ILogin login, ITokenStore tokenStore)
        {
            _logger = logger;  // filelogger
            _login = login;
            _tokenStore = tokenStore;
        }

        protected readonly ITokenStore _tokenStore;
        protected T _options { get; set;}
        protected readonly ILogger _logger;
        protected readonly ILogin _login;

        protected void VerboseWrite(string message)
        {
            if (_options.Verbose && !_options.Silent) {
                Console.WriteLine(message);
            }
            _logger.Information(message);
        }

        protected void WriteLine(string message)
        {
            if (!_options.Silent) {
                Console.WriteLine(message);
            }
            _logger.Information(message);
        }

        public int Run(T options)
        {
            try
            {
                _options = options;
                ValidateOptions();
                CheckLogin().GetAwaiter().GetResult();
                DoWork().GetAwaiter().GetResult();
                return 0;
            } catch (Exception ex) {
                _logger.Error(ex, "An Unhandled Error Occured.");
                WriteLine("Error:  Please check you inputs and try again.  Check logfile for more details.");
                return 1;
            }
        }

        protected abstract Task DoWork();

        public virtual void ValidateOptions()
        {
            if (_options.Verbose && _options.Silent) {
                string message = "The 'silent' and 'verbose' options are mutually exclusive.  Please use only one of those options and try again.";
                Console.WriteLine(message);
                _logger.Error(message);
            }
        }

        protected async Task CheckLogin()
        {
            if (_options.NoCache) {
                await Login();
            }   
        }

        protected async Task Login()
        {
            TokenData tokenData = await _login.Login(_options.Scope);
            await StoreTokenData(tokenData);
        }

        
        private async Task StoreTokenData(TokenData tokenData)
        {
            AccessTokenUtility tokenUtil = new AccessTokenUtility(tokenData.AccessToken);
            CurrentUserPermissions userScope = new CurrentUserPermissions{
                UserId = tokenUtil.GetUserId(),
                Permissions = tokenUtil.GetPermissions()
            };
            await SerializationExtensions.StoreSingletonToJsonFile(userScope);
        }



    }
}