using Newtonsoft.Json;
using System;

namespace HUBService.Domain.Models
{
    public class Document
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("incdate")]
        public DateTime Incdate { get; set; }

        [JsonProperty("image_front")]
        public string Image_front { get; set; }

        [JsonProperty("image_back")]
        public string Image_back { get; set; }

        [JsonProperty("complementaryinfo")]
        public string Complementaryinfo { get; set; }
    }
}