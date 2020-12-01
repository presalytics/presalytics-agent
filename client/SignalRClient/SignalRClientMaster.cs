using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using Serilog;
using Microsoft.Extensions.DependencyInjection;
using shared.Extensions;
using shared.Exceptions;
using shared.Models;
using Microsoft.Net.Http.Headers;
using client.Services;

namespace client.SignalRClient
{
    public interface ISignalRClientMaster
    {

        Task SendStoryToServer(string story);
        Task SyncAgent(WorkspaceAgent agent);

        Task<HubConnection> Connect(string url);
        Task<HubConnection> Connect();
        string GetHubUrl();
        void SetHubUrl(string hubUrl);

    }
    public class SignalRClientMaster : ISignalRClientMaster
    {
        public string Url { get; set; }
        private ITokenProvider _tokenProvider {get; set;}
        private ILogger _logger { get; set;}

        private Dictionary<string, HubConnection> _userConnections { get; set;}
        private IHttpContextAccessor _httpContextAccessor { get; set;}

        private string _hubUrl { get; set;}

        public SignalRClientMaster(IServiceProvider sp)
        {
            
            _logger = sp.GetRequiredService<ILogger>();
            _tokenProvider = sp.GetRequiredService<ITokenProvider>();
            _httpContextAccessor = sp.GetRequiredService<IHttpContextAccessor>();
            _userConnections = new Dictionary<string, HubConnection>();
        }

        public string GetHubUrl()
        {
            return _hubUrl;
        }

        public void SetHubUrl(string hubUrl)
        {
            _hubUrl = hubUrl;
        }

        public async Task<HubConnection> Connect()
        {
            if (_hubUrl == null) {
                throw new PresalyticsHubConnectionException("HubUrl must be set to use this parameterless connection method.");
            }
            return await Connect(_hubUrl); 
        }

    
        public async Task<HubConnection> Connect(string url)
        {
            Url = url;
            HubConnection Connection = new HubConnectionBuilder()
                .WithUrl(Url, options => {
                    options.AccessTokenProvider = () => {
                        return Task.FromResult(_tokenProvider.GetBearerToken());
                    };
                })
                .WithAutomaticReconnect()
                .Build();

            // Define methods that get called by Hub using Connection.On
            // The method name is the 1st argument and is case-senstive
            // Proceeding arguments are dynamic
            Connection.On<string>("UpdateStory", (storyId) =>
            {
                Log.Information("Received UpdateStory Request for " + storyId);

            });

            Connection.On("AgentSync", () => {
                Log.Information("Received AgentSync request from Hub");
                AgentSyncEventArgs args = new AgentSyncEventArgs();
                OnMessageReceived(args);
            });

            Connection.Closed += async (error) =>
            {
                System.Diagnostics.Debug.Assert(Connection.State == HubConnectionState.Disconnected);

                await ConnectWithRetryAsync(Connection);

            };


            await ConnectWithRetryAsync(Connection);
            string userId = _httpContextAccessor.HttpContext.GetPresalyticsUserId();
            _userConnections.Add(userId, Connection);
            return Connection;

        }
    
        public event EventHandler MessageReceived;
        protected virtual void OnMessageReceived(EventArgs e)
        {
            EventHandler handler = MessageReceived;
            handler?.Invoke(this, e);
        }

        public async Task SendStoryToServer(string story)
        {
            HubConnection conn = await GetHubConnection();
            await conn.InvokeAsync("SetStory", story);
            Log.Information("Story Updated");
        }

        public async Task SyncAgent(WorkspaceAgent agent)
        {
            HubConnection conn = await GetHubConnection();
            await conn.InvokeAsync("AgentSync", agent.AgentId, agent.Name);
            Log.Information("AgentId {AgentId} updated at hub", agent.AgentId);
        }

        public async Task<HubConnection> GetHubConnection(string userId)
        {
            HubConnection conn;

            if (!_userConnections.TryGetValue(userId, out conn))
            {
                conn = await Connect();
            }

            if (conn.State == HubConnectionState.Disconnected) 
            {
                await ConnectWithRetryAsync(conn);
            }
            return conn;
        }

        public async Task<HubConnection> GetHubConnection()
        {
            string userId = _httpContextAccessor.HttpContext.GetPresalyticsUserId();
            return await GetHubConnection(userId);
        }

        public async Task<bool> ConnectWithRetryAsync(HubConnection connection)
        {
            // Keep trying to until we can start or the token is canceled.
            while (true)
            {
                try
                {
                    await connection.StartAsync();
                    System.Diagnostics.Debug.Assert(connection.State == HubConnectionState.Connected);
                    return true;
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Failed to connect to SignalR Hub.");
                    // Failed to connect, trying again in 5000 ms.
                    System.Diagnostics.Debug.Assert(connection.State == HubConnectionState.Disconnected);
                    await Task.Delay(5000);
                }
            }
        }
    }
}