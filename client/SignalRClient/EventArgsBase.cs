using System;

namespace client.SignalRClient
{
    public class EventArgsBase : EventArgs
    {
        public virtual string Action { get; set;}

        public EventArgsBase() {}
        public EventArgsBase(string actionName)
        {
            Action = actionName;
        }
    }

    
}