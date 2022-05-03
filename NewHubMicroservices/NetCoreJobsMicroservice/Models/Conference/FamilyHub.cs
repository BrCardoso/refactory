using Commons.Base;

using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace NetCoreJobsMicroservice.Models
{
	public class FamilyHub
	{
		public class Member
		{
			public Guid personguid { get; set; }
			public string kinship { get; set; }
			public string typeuser { get; set; }
			public string name { get; set; }
			public string sequencial { get; set; }
			public string cpf { get; set; }
			public string origin { get; set; }
			public string maritalstatus { get; set; }
			public string gender { get; set; }
			public DateTime birthdate { get; set; }
			public string mothername { get; set; }
			public DateTime? blockDate { get; set; }
			public string blockReason { get; set; }
			public List<Benefitinfo> benefitinfos { get; set; }
		}

		public string aggregator { get; set; }
		public Guid guid { get; set; }
		public Guid hubguid { get; set; }
		public Guid personguid { get; set; }

		[JsonPropertyName("family")]
		public List<Member> members { get; set; }
	}
}