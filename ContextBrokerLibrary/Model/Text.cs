using Newtonsoft.Json;

namespace ContextBrokerLibrary.Model
{
    public class Text
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("value")]
        public string Value { get; set; }
    }
}