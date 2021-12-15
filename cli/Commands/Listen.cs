namespace cli.Commands
{
    using CommandLine;
    using System;
    using System.Threading.Tasks;
    using Serilog;
    using presalytics.Services.Auth;
    using presalytics;
    using presalytics.Services;
    using CloudNative.CloudEvents;
    using System.Text.Json;
    using System.Net.Http;
    using CloudNative.CloudEvents.SystemTextJson;
    using CloudNative.CloudEvents.Http;
    using presalytics.Models;

    [Verb("listen", HelpText = "Get your cloudevents from the Presalytics Event Stream.")]
    public class ListenOptions : CommandOptionsBase 
    {
        [Option('f', "forward-to", HelpText = "Foward the presaltyics event stream to a Url.  Helpful for integrating presalytics' events into other applcations.")]
        public Uri ForwardTo { get; set;}
    }

    public class ListenCommandService : CommandServiceBase<ListenOptions>, ICommandService<ListenOptions>
    {
        private ISignalREventsClient _sr;
        public IDefaultEventDispatcher Dispatcher;
        private IHttpClientFactory _clientFactory;
        public ListenCommandService(ILogger logger, ILogin login, ITokenStore tokenStore, ISignalREventsClient signalRClient, IDefaultEventDispatcher dispatcher, IHttpClientFactory clientFactory) : base(logger, login, tokenStore) 
        {
            _sr = signalRClient;
            Dispatcher = dispatcher;
            _clientFactory = clientFactory;

        }

        protected override async Task DoWork()
        {
    
            BindEvents();
            await _sr.ConnectWithRetryAsync();
        }

        private void BindEvents()
        {
            Dispatcher.OnCloudEvent += WriteEventHandler;
            if (this._options.ForwardTo != null) {
                Dispatcher.OnCloudEvent += ForwardEventHandler;
                if (!this._options.Silent) {
                    Console.WriteLine("Forwarding events to: <{0}>", this._options.ForwardTo);
                }
            }

            _sr.Connected += WriteConnectedMessage;
        }

        private void WriteConnectedMessage(object sender, EventArgs e)
        {
            if (!this._options.Silent) {
                Console.WriteLine("Connected to Presalytics event hub at < {0} >. Listening for Events...", _sr.HubUrl);
            }
        }

        private void WriteReconnectingMessage(object sender, EventArgs e)
        {
            if (!this._options.Silent) {
                Console.WriteLine("Connected to Presalytics event hub at < {0} >. Listening for Events...", _sr.HubUrl);
            }
        }

        private void WriteEventHandler(object sender, CloudEvent e)
        {
            if (!this._options.Silent) {
                Console.WriteLine("Event Received: Type = '{0}', Id = '{1}'", e.Type, e.Id);
                if (this._options.Verbose) {
                    JsonSerializerOptions jsonOptions = SerializationExtensions.GetDefaultJsonSerializerOptions();
                    jsonOptions.WriteIndented = true;
                    string eventJson = JsonSerializer.Serialize(e, jsonOptions);
                    Console.WriteLine(eventJson);
                }
            }
            
        }

        private async void ForwardEventHandler(object sender, CloudEvent e)
        {
            await ForwardEvent(e);
        }

        private async Task ForwardEvent(CloudEvent e)
        {
            try {
                HttpClient client = _clientFactory.CreateClient();
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("User-Agent", "presalytics-cli");
                CloudEventFormatter formatter = new JsonEventFormatter();
                HttpRequestMessage request = new HttpRequestMessage
                {
                    Method = HttpMethod.Post,
                    Content = e.ToHttpContent(ContentMode.Structured, formatter)
                };

                    HttpResponseMessage msg = await client.SendAsync(request);
                    if (msg.IsSuccessStatusCode) {
                        if (this._options.Verbose) Console.WriteLine("Event successfully forwarded to < {0} > ", this._options.ForwardTo);
                    } else {
                        string message = string.Format("Event forwarding failed. Endpoint {0} responded with status code {1}", this._options.ForwardTo, msg.StatusCode); 
                        if (!this._options.Silent) Console.WriteLine(message);
                        _logger.Information(message);
                    }
            } catch (HttpRequestException) {
                string message = string.Format("No response from forward url endpoint.  Is a server listening at < {0} >", this._options.ForwardTo);
                _logger.Information(message);
                if (!this._options.Silent) Console.WriteLine(message);
            } catch (Exception ex) {
                string message = string.Format("Unknown error sending message to < {0} >.", this._options.ForwardTo);
                _logger.Error(ex, message);
                if (!this._options.Silent) Console.WriteLine(message);
            } 

        }
    }
}