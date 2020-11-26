using Microsoft.AspNetCore.SignalR;
using System.Linq;

namespace hub.Services
{
    public class PresalyticsUserIdProvider : IUserIdProvider
    {
        public virtual string GetUserId(HubConnectionContext connection)
        {
            return connection.User?.Claims.Where(c => c.Type == "https://api.presalytics.io/api_user_id").First().Value;
        }
    }

}