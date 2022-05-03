using Commons.Base;

using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
	public class BenefitiaryDB
	{
		public List<Benefitinfo> benefitinfos { get; set; }
		public DateTime? blockDate { get; set; }
		public string blockReason { get; set; }
		public string kinship { get; set; }
		public string origin { get; set; }
		public Guid personguid { get; set; }
		public string sequencial { get; set; }
		public string typeuser { get; set; }
	}

	public class FamilyDB
	{
		public Guid guid { get; set; }
		public Guid hubguid { get; set; }
		public Guid personguid { get; set; }
		public string aggregator { get; set; }
		public string docType { get; set; } = "Family";
		public List<BenefitiaryDB> family { get; set; }
	}
}