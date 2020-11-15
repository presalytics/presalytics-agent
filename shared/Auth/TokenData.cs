using System;
using Newtonsoft.Json;

namespace shared.Auth
{
    public class TokenData
    {
        [JsonProperty(PropertyName = "access_token")]
        public string AccessToken { get; set;}

        [JsonProperty(PropertyName = "expires_in")]
        public int ExpiresIn { get; set; }

        [JsonProperty(PropertyName = "not_before_policy")]
        public int NotBeforePolicy { get; set;}

        [JsonProperty(PropertyName = "refresh_expires_in")]
        public int RefreshExpiresIn { get; set;}

        [JsonProperty(PropertyName = "refresh_token")]
        public string RefreshToken { get; set;}

        [JsonProperty(PropertyName = "scope")]
        public string Scope { get; set;}

        [JsonProperty(PropertyName = "session_state")]

        public Guid SessionState { get; set;}

        [JsonProperty(PropertyName = "token_type")]
        public string TokenType { get; set;}

    }

    // Use stack memory to store a single token as a singleton.  Admittedly, this is lazy.
    public static class TokenDataMgr
    {
        public static TokenData Token { get; set;}

        public static DateTime AccessTokenExpiry { get; set;}

        static TokenDataMgr()
        {
            Token = null;
            AccessTokenExpiry = DateTime.Now;
        }

        public static void PutToken(TokenData token)
        {
            Token = token;
            SetAccessTokenExpiry();
        }

        public static TokenData GetTokenData()
        {
            return Token;
        }

        public static bool IsExpired()
        {
            return AccessTokenExpiry < DateTime.Now;
        }

        private static void SetAccessTokenExpiry()
        {
            TimeSpan _spanTime = new TimeSpan(0, 0, Token.ExpiresIn) - new TimeSpan(0, 0, 1); // Shaving a second to compensate for request delay
            AccessTokenExpiry = DateTime.Now + _spanTime;
        }
        
        
    }
}