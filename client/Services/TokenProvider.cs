using Microsoft.Net.Http.Headers;
using Microsoft.AspNetCore.Http;

namespace client.Services
{
    public interface ITokenProvider
    {  
        string GetBearerToken();
    }

    public class TokenProvider : ITokenProvider
    {
        private IHttpContextAccessor _httpContextAccessor { get; set;}
        public TokenProvider(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string GetBearerToken()
        {
            return _httpContextAccessor.HttpContext.Request.Headers["Authorization"];
        }
    }
}