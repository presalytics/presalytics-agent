using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using Serilog;
using CorrelationId;
using System.Text;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;


namespace shared.Auth
{
    public class PresalyticsCookieMiddleware : IMiddleware
    {
        private IDistributedCache _redis;
        private HttpClient _siteApiClient;
        private string _correlationHeader;
        private string _correlationId;
        private IServiceAccountTokenManager _tokenMgr;
        private ILogger _logger;
        private string _userTknPrefix;


        public PresalyticsCookieMiddleware(IServiceProvider services)
        {

            _redis = services.GetService<IDistributedCache>();
            IHttpClientFactory _clientFactory = services.GetService<IHttpClientFactory>();
            _siteApiClient = _clientFactory.CreateClient("site-api-client");
            _tokenMgr = services.GetService<IServiceAccountTokenManager>();
            ICorrelationContextAccessor correlationContext = services.GetService<ICorrelationContextAccessor>();
            _correlationHeader = correlationContext.CorrelationContext.Header;
            _correlationId = correlationContext.CorrelationContext.CorrelationId;
            _logger = services.GetRequiredService<ILogger>();
            IConfiguration config = services.GetRequiredService<IConfiguration>();
            _userTknPrefix = "ddapi-user-" + config.GetValue<string>("DDAPI:USER_TKN_PREFIX", "");

        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate _next)
        {
            // Check if there's a bearer token in header.  If so, bypass this middleware
            try 
            {
                if (context.Request is HttpRequest)
                {
                    if (!context.Request.Headers.ContainsKey("Authorization"))
                    {
                        string sessionId = null;
                        string userAccessToken = null;

                        if (context.Request.Cookies.TryGetValue("presalytics_session_id", out sessionId))
                        {
                            userAccessToken = await getUserAccessTokenAsync(sessionId);
                            if (userAccessToken == null) {
                                userAccessToken = await _tokenMgr.GetPublicUserToken();
                            }
                            if (userAccessToken != null ) {
                                context = updateRequestContext(context, userAccessToken);
                            }
                        }
                    }
                }
            } catch (Exception ex) {
                _logger.Error(ex, "Presalytics Cookie Middleware Failure.");
            }
            
            await _next(context);
        }


        private async Task<string> getUserAccessTokenAsync(string sessionId) 
        {
            string accessToken = await getCachedUserToken(sessionId);
            if (accessToken == null) 
            {
                string clientCredentialsAT = await _tokenMgr.GetToken();
                _siteApiClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", clientCredentialsAT);
                _siteApiClient.DefaultRequestHeaders.Add(_correlationHeader, _correlationId);
                Uri endpoint = new Uri("user/session-token/", UriKind.Relative);
                SessionKey data = new SessionKey();
                data.SessionId = sessionId;
                string jsonInString = JsonConvert.SerializeObject(data);
                StringContent body = new StringContent(jsonInString, Encoding.UTF8, "application/json");
                HttpResponseMessage _resp = await _siteApiClient.PostAsync(endpoint, body);
                if ((int)_resp.StatusCode == 200) {
                    string _jsonString = await _resp.Content.ReadAsStringAsync();
                    TokenData tokenData = JsonConvert.DeserializeObject<TokenData>(_jsonString);
                    accessToken = tokenData.AccessToken;
                    await Task.Run( () => cacheUserToken(accessToken, sessionId));
                }
            }
            return accessToken;
        }

        private void cacheUserToken(string accessToken, string sessionId) 
        {
            string key = _userTknPrefix + sessionId;
            byte[] val = Encoding.UTF8.GetBytes(accessToken);
            _redis.Set(key, val, new DistributedCacheEntryOptions() {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(1800)
            }); 
        }

        private async Task<string> getCachedUserToken(string sessionId)
        {
            string key = _userTknPrefix + sessionId;
            return await _redis.GetStringAsync(key);
        }

        private HttpContext updateRequestContext(HttpContext context, string userAccessToken)
        {
            context.Request.Headers.Add("Authorization", "Bearer " + userAccessToken);
            return context;
        }
    }

    public static class PresalyticsCookieMiddlewareExtensions
    {
        public static IApplicationBuilder UsePresalyticsCookieMiddleware(this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<PresalyticsCookieMiddleware>();
        }
    }
}