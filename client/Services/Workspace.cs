using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Serilog;
using shared.Models;

namespace client.Services
{
    public interface IWorkspace
    {
        Task RequestStoryUpdate(string storyId);
        Task<WorkspaceAgent> GetAgentAsync();
    }
    public class Workspace : IWorkspace
    {
        private ILogger Logger { get; set;}
        private HttpClient Client { get; set;}
        public Workspace(ILogger logger, IHttpClientFactory clientFactory)
        {
            Logger = logger;
            Client = clientFactory.CreateClient("workspace");
        }

        public async Task RequestStoryUpdate(string storyId)
        {
            await Client.GetAsync("/update-story/" + storyId);
        }

        public async Task<WorkspaceAgent> GetAgentAsync()
        {
            Logger.Information("Getting AgentId From workspace Service.");
            try
            {
                WorkspaceAgent agent =  await Client.GetFromJsonAsync<WorkspaceAgent>("/agent");
                return agent;
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }
    }
}