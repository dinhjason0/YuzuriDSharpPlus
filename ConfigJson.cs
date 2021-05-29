
using Newtonsoft.Json;

namespace Yuzuri
{
    public struct ConfigJson
    {
        [JsonProperty("Token")]
        public string Token { get; set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; set; }
    }
}