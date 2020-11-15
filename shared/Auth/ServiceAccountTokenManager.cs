using System;
using System.Net.Http;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Serilog;
using Newtonsoft.Json;
using System.Text;

namespace shared.Auth
{
    public interface IServiceAccountTokenManager
    {
        Task<string> GetToken();

        Task<string> GetPublicUserToken();
    }

    public class ServiceAccountTokenManager : IServiceAccountTokenManager
    {
        private HttpClient _tokenClient;
        private IDistributedCache _redis;
        private FormUrlEncodedContent _tokenRequestPayload;
        private ILogger _logger;
        private string _tokenKey;
        private string _publicUserTokenKey;
        private FormUrlEncodedContent _publicTokenPayload;
        

        public ServiceAccountTokenManager(IServiceProvider services)
        {
            IHttpClientFactory clientFactory = services.GetRequiredService<IHttpClientFactory>();
            _tokenClient = clientFactory.CreateClient("service-account-token");
            _redis = services.GetRequiredService<IDistributedCache>();
            _logger = services.GetRequiredService<ILogger>();
            

            IConfiguration config = services.GetRequiredService<IConfiguration>();
            string _clientId = config.GetValue<string>("DDAPI:CLIENT_ID");
            string _clientSecret = config.GetValue<string>("DDAPI:CLIENT_SECRET");
            _tokenKey = "ddapikey-" + config.GetValue<string>("DD_API:CC_TOKEN_KEY");
            _publicUserTokenKey = "ddapi-publicuser-" + config.GetValue<string>("PUBLIC_USER_CACHE_KEY", "");
            string _publicUserName = config.GetValue<string>("PUBLIC_USER_NAME");
            string _publicUserPassword = config.GetValue<string>("PUBLIC_USER_PASSWORD");

            
            List<KeyValuePair<string, string>> _tokenParams = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("grant_type", "client_credentials"),
                new KeyValuePair<string, string>("audience", "https://api.presalytics.io/")
            };

            List<KeyValuePair<string, string>> _publicTokenParams = new List<KeyValuePair<string, string>>() {
                new KeyValuePair<string, string>("client_id", _clientId),
                new KeyValuePair<string, string>("client_secret", _clientSecret),
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("audience", "https://api.presalytics.io/"),
                new KeyValuePair<string, string>("scope", "website:create-sessions"),
                new KeyValuePair<string, string>("username", _publicUserName),
                new KeyValuePair<string, string>("password", _publicUserPassword)

            };

            _tokenRequestPayload = new FormUrlEncodedContent(_tokenParams);

            _publicTokenPayload = new FormUrlEncodedContent(_publicTokenParams);

        }

        public async Task<string> GetToken()
        {
            string token = await _redis.GetStringAsync(_tokenKey);
            if (token == null) {
                token = await GetNewToken();
            }
            return token;
        }

        private async Task<string> GetNewToken()
        {
            return await GetNewAccessToken(_tokenRequestPayload, _tokenKey);
        }

        public async Task<string> GetPublicUserToken()
        {
            string token = await _redis.GetStringAsync(_publicUserTokenKey);
            if (token == null) {
                token = await GetNewPublicUserToken();
            }
            return token;
        }

        public async Task<string> GetNewPublicUserToken() 
        {
            return await GetNewAccessToken(_publicTokenPayload, _publicUserTokenKey);
        }

        public async Task<string> GetNewAccessToken(FormUrlEncodedContent payload, string cacheKey)
        {
            TokenData _tokenData = null;
            HttpResponseMessage _tokenRepsonse = await _tokenClient.PostAsync(new Uri("", UriKind.Relative), payload);
            if ((int)_tokenRepsonse.StatusCode == 200) 
            {
                string _jsonString = await _tokenRepsonse.Content.ReadAsStringAsync();
                _logger.Information("Retrived token data: {0}", _jsonString);
              
                try
                {
                    _tokenData = JsonConvert.DeserializeObject<TokenData>(_jsonString);
                    byte[] jsonBytes = Encoding.UTF8.GetBytes(_tokenData.AccessToken);
                    _redis.Set(cacheKey, jsonBytes, new DistributedCacheEntryOptions() {
                        AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(82800)
                    });       
                }
                catch (Exception ex)
                {
                    _logger.Error(ex.Message);
                }
            }
            return _tokenData.AccessToken;

        }
    }
}