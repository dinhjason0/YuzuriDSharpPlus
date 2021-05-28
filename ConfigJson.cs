
using Newtonsoft.Json;

namespace Yuzuri
{
    public struct ConfigJson
    {
        [JsonProperty("Token")]
        public string Token { get; private set; }

        [JsonProperty("Prefix")]
        public string Prefix { get; private set; }

        [JsonProperty("NadekoBotDB")]
        public string NadekoBotDB { get; private set; }

        [JsonProperty("NQRate")]
        public double NQRate { get; private set; }

        [JsonProperty("NQPRate")]
        public double NQPRate { get; private set; }

        [JsonProperty("HQRate")]
        public double HQRate { get; private set; }

        [JsonProperty("HHQRate")]
        public double HHQRate { get; private set; }

        [JsonProperty("UQRate")]
        public double UQRate { get; private set; }
    }
}
