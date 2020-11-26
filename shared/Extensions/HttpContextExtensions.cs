using Microsoft.AspNetCore.Http;
using System.Linq;
using System;

namespace shared.Extensions
{
    public static class HttpContextExtensions
    {
        public static string GetPresalyticsUserId(this HttpContext httpContext)
        {
            string ret;
            try
            {
                ret = httpContext.User?.Claims.Where(c => c.Type == "https://api.presalytics.io/api_user_id").First().Value;
            } 
            catch (Exception)
            {
                ret = Guid.Empty.ToString();
            }
            return ret;

        }
    }
}