using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System.Net.Http;
using Serilog;
using hub.Services;

/// These methods are called by the client and run on the server
namespace hub.Hubs
{
    public class AgentHub : Hub<IAgentClient>
    {
        private ILogger _logger { get; set;}
        private IAgentManager _agentManager { get; set;}
        public AgentHub(ILogger logger, IAgentManager agentManager)
        {
            _logger = logger;
            _agentManager = agentManager;
        }
        public Task SetStory(string storyId)
        {
            _logger.Information("Story Set Method Called");
            return Task.CompletedTask;
        }

        public Task AgentStoryUpdate(string storyId, string connectionId)
        {
            return Clients.Client(connectionId).UpdateStory(storyId);
        }

        public Task GetStatus(string userId)
        {
            return Clients.User(userId).AgentSync();
        }

        public Task SyncAgent(string agentId, string agentName)
        {   
            return _agentManager.SyncAgent(new Guid(agentId), agentName);
        }



        public async Task AgentSync(string agentId, string name)
        {
            await _agentManager.SyncAgent(Guid.Parse(agentId), name);
        }


    }
}