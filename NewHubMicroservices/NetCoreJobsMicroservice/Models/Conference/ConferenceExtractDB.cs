using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
	public class ConferenceExtractDB
	{
		public class Used
		{
			public bool status { get; set; }
			public Guid? invoicevalidationguid { get; set; }
			public DateTime? date { get; set; }
		}

		public class Source
		{
			public Guid invoicevalidationguid { get; set; }
			public DateTime date { get; set; }
		}

		public class Extract
		{
			public double value { get; set; }
			public string type { get; set; }
			public Used used { get; set; }
			public Source source { get; set; }
		}

		public class Account
		{
			public double currentvalue { get; set; }
			public List<Extract> extract { get; set; }
		}

		public class ConferenceExtract
		{
			public string docType { get; set; } = "LeagerAccount";
			public Guid? guid { get; set; }
			public Guid? hubguid { get; set; }
			public string aggregator { get; set; }
			public Guid? providerguid { get; set; }
			public Account account { get; set; }
		}
	}
}