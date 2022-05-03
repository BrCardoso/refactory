using Newtonsoft.Json;
using System;

namespace HUBService.Domain.Models
{
    public class Benefit
	{
		[JsonProperty("providerguid")]
		public Guid Providerguid { get; set; }
		[JsonProperty("providerid")]
		public string Providerid { get; set; }
		[JsonProperty("productcode")]
		public string Productcode { get; set; }
		[JsonProperty("contractnumber")]
		public string Contractnumber { get; set; }
		[JsonProperty("cardnumber")]
		public string Cardnumber { get; set; }
		[JsonProperty("blockdate")]
		public DateTime? Blockdate { get; set; }
		[JsonProperty("BlockReason")]
		public string BlockReason { get; set; }
	}
}