using System;
using Newtonsoft.Json;

namespace shared.Auth
{
    public class SessionKey
    {
        [JsonProperty(PropertyName = "session_id")]
        public string SessionId { get; set;}
    }
}