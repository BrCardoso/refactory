using Newtonsoft.Json;

namespace HUBService.Domain.Models
{
    public class Address
    {
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("zipcode")]
        public string Zipcode { get; set; }
        [JsonProperty("patiotype")]
        public string Patiotype { get; set; }
        [JsonProperty("street")]
        public string Street { get; set; }
        [JsonProperty("number")]
        public int Number { get; set; }
        [JsonProperty("addicionalinfo")]
        public string Addicionalinfo { get; set; }
        [JsonProperty("neighborhood")]
        public string Neighborhood { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
        [JsonProperty("state")]
        public string State { get; set; }
        [JsonProperty("country")]
        public string Country { get; set; }
        [JsonProperty("isdefault")]
        public bool? Isdefault { get; set; }
    }
}