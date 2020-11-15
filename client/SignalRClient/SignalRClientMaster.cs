using Microsoft.AspNetCore.SignalR.Client;
using System.Threading.Tasks;
using System;
using Serilog;
using shared;

namespace client.SignalRClient
{
    public interface ISignalRClientMaster
    {
        HubConnection Connection {get; set;}

        Task SendStoryToServer(string story);
        Task SyncAgent(string agentId);

    }
    public class SignalRClientMaster : ISignalRClientMaster
    {
        public string Url { get; set; }
        public HubConnection Connection { get; set;}

    
        public SignalRClientMaster(string url)
        {
            Url = url;
            Connection = new HubConnectionBuilder()
                .WithUrl(Url)
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

            Connection.Closed += error =>
            {
                System.Diagnostics.Debug.Assert(Connection.State == HubConnectionState.Disconnected);

                // Notify users the connection has been closed or manually try to restart the connection.

                return Task.CompletedTask;
            };


            Task.Run( () => ConnectWithRetryAsync(Connection));

        }
    
        public event EventHandler MessageReceived;
        protected virtual void OnMessageReceived(EventArgs e)
        {
            EventHandler handler = MessageReceived;
            handler?.Invoke(this, e);
        }

        public async Task SendStoryToServer(string story)
        {
            await Connection.InvokeAsync("SetStory", story);
            Log.Information("Story Updated");
        }

        public async Task SyncAgent(string agentId)
        {
            await Connection.InvokeAsync("AgentSync", agentId);
            Log.Information("AgentId {AgenetId} updated at hub", agentId);
        }

        public static async Task<bool> ConnectWithRetryAsync(HubConnection connection)
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
                catch
                {
                    // Failed to connect, trying again in 5000 ms.
                    System.Diagnostics.Debug.Assert(connection.State == HubConnectionState.Disconnected);
                    await Task.Delay(5000);
                }
            }
        }
    }
}