using System.Threading.Tasks;

namespace hub.Hubs
{
    public interface IAgentClient
    {
        Task UpdateStory(string storyId);

        Task AgentSync();
    }
}