#nullable enable
using System;

namespace client.SignalRClient
{
    public class  AgentSyncEventArgs : EventArgsBase
    {
        public new string Action = "AgentSync";

        public AgentSyncEventArgs()
        {
            
        }
    }

    
}