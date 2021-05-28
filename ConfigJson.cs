
using Newtonsoft.Json;

namespace Yuzuri
{
    public struct ConfigJson
    {
        [JsonProperty("Token")]
        public string Token { get; private set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("PlayerFilePath")]
        public string PlayerFilePath { get; private set; }
    }
}