using Newtonsoft.Json;

namespace HUBService.Domain.Models
{
    public class Emailinfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("email")]
        public string Email { get; set; }
        [JsonProperty("isdefault")]
        public bool? Isdefault { get; set; }
    }
}