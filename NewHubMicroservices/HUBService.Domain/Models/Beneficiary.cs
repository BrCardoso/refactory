using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Text;

namespace HUBService.Domain.Models
{
	public class Beneficiary
	{
		public string Origin { get; set; }
		public string Sequencial { get; set; }
		public string Kinship { get; set; }
		public string Typeuser { get; set; }
		public DateTime? BlockDate { get; set; }
		public string BlockReason { get; set; }

		[JsonProperty("benefitinfos")]
		public List<Benefit> Benefitinfos { get; set; }
	}
}
