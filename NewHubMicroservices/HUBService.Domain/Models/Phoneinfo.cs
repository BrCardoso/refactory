using Newtonsoft.Json;

namespace HUBService.Domain.Models
{
    public class Phoneinfo
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("countrycode")]
        public string Countrycode { get; set; }
        [JsonProperty("area")]
        public string Area { get; set; }
        [JsonProperty("phonenumber")]
        public string Phonenumber { get; set; }
        [JsonProperty("extension")]
        public string Extension { get; set; }
        [JsonProperty("isdefault")]
        public bool? Isdefault { get; set; }
    }
}