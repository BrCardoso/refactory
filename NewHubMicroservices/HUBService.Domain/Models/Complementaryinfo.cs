using Newtonsoft.Json;

namespace HUBService.Domain.Models
{
    public class Complementaryinfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("value")]
        public string Value { get; set; }
    }
}