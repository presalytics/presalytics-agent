using System;
using System.Net.Http;
using System.Threading.Tasks;
using Serilog;

namespace client.Services
{
    public interface IWorkspace
    {
        Task RequestStoryUpdate(string storyId);
        Task<string> GetAgentIdAsync();
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

        public async Task<string> GetAgentIdAsync()
        {
            Logger.Information("Getting AgentId From workspace Service.");
            try
            {
                HttpResponseMessage resp =  await Client.GetAsync("/agent");
                int statusCode = (int)resp.StatusCode;
                if (200 <= statusCode && statusCode<= 299)
                {
                    return await resp.Content.ReadAsStringAsync();
                }
                else 
                {
                    throw new HttpRequestException("Error retreiving data from workspace service.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex, ex.Message);
                throw ex;
            }
        }
    }
}