using System;
using System.Threading.Tasks;
using Serilog;
using client.SignalRClient;

namespace client.Services
{
    public interface ISocketBroker
    {
        ISignalRClientMaster Socket { get; set;}
        IWorkspace Workspace { get; set;}

        Task HandleMessageReceivedEvent(object sender, EventArgs e);
    }

    public class SocketBroker : ISocketBroker
    {
        public ISignalRClientMaster Socket { get; set; }
        public IWorkspace Workspace { get; set;}

        private ILogger _logger { get; set;}
        public SocketBroker(IWorkspace workspace, ILogger logger, ISignalRClientMaster socket)
        {
            Workspace = workspace;
            Socket = socket;
            _logger = logger;
        }

        public async Task HandleMessageReceivedEvent(object sender, EventArgs e)
        {
            _logger.Debug("Handling command issued from hub from hub");
            
            if (e is AgentSyncEventArgs)
            {
                AgentSyncEventArgs agentArgs = (AgentSyncEventArgs)e;
                await Workspace.GetAgentIdAsync();
                
            } 
        }

    }
}