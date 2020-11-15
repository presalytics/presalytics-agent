using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Serilog;
using System;
using shared.Models;
using System.Net.Http.Json;

namespace hub.Services
{
    public interface IAgentManager 
    {
        Task SyncAgent(Guid agentId);
        
        Task<List<UserAgent>> GetUserAgents();
    }
    public class AgentManager : IAgentManager
    {
        private ILogger _logger { get; set;}
        private HttpClient _websiteClient { get; set;}
        public AgentManager(ILogger logger, IHttpClientFactory clientFactory)
        {
            _logger = logger;
            _websiteClient = clientFactory.CreateClient("website");

        }

        public async Task<List<UserAgent>> GetUserAgents()
        {
            List<UserAgent> userAgents = await _websiteClient.GetFromJsonAsync<List<UserAgent>>("/user/agents");
            return userAgents;
        }

        public async Task SyncAgent(Guid agentId)
        {
            var agent = new UserAgent() {
                AgentId = agentId
            };
            await _websiteClient.PostAsJsonAsync<UserAgent>("/user/agents", agent);
        }
    }
}