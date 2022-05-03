using System;
using System.Collections.Generic;

using static Commons.Helpers;

namespace Commons.Base
{
	public class Beneficiary : Person
	{
		public string Origin { get; set; }
		public string Sequencial { get; set; }
		public string Typeuser { get; set; }
		public DateTime? BlockDate { get; set; }
		public string BlockReason { get; set; }
		public List<Benefitinfo> Benefitinfos { get; set; }
	}

	public class BeneficiaryAddInfo
	{
		public string Origin { get; set; }
		public string Sequencial { get; set; }
		public string Kinship { get; set; }
		public string Typeuser { get; set; }
		public DateTime? BlockDate { get; set; }
		public string BlockReason { get; set; }
		public List<Benefitinfo> benefitinfos { get; set; }
	}

	public partial class Benefitinfo : IBenefitinfo
	{

		public Guid providerguid { get; set; }
		public string providerid { get; set; }
		public string providerproductCode { get; set; }
		public string product { get; set; }
		[NotEmpty]
		public string productcode { get; set; }
		[NotEmpty]
		public string contractnumber { get; set; }
		public string cardnumber { get; set; }
		public DateTime startdate { get; set; }
		public DateTime? blockdate { get; set; }
		public string BlockReason { get; set; }
		public bool Sync { get; set; }
		public bool Synced { get; set; }
		public List<Complementaryinfo> complementaryinfos { get; set; }
		public string subsegment { get; set; }
		public Transference Transference { get; set; }
		public string ReissueReason { get; set; }
	}
	public class Transference	{
		public string ProviderProductCode { get; set; }
		public string Product { get; set; }
		public DateTime StartDate { get; set; }
	}

	public interface IBenefitinfo
	{
		public Guid providerguid { get; set; }
		public string providerid { get; set; }
		public string productcode { get; set; }
		public string contractnumber { get; set; }
		public string cardnumber { get; set; }
		public DateTime? blockdate { get; set; }
		public string BlockReason { get; set; }
	}

	public class BenefitTransactionModel
	{
		public class BenefitinfoBlock : Benefitinfo
		{
			public string product { get; set; }
			public string providerName { get; set; }
		}

		public class Family
		{
			public string origin { get; set; }
			public Guid personguid { get; set; }
			public string personName { get; set; }
			public string sequencial { get; set; }
			public string kinship { get; set; }
			public string typeuser { get; set; }
			public DateTime? blockDate { get; set; }
			public string blockReason { get; set; }
			public List<BenefitinfoBlock> benefitinfos { get; set; }
		}

		public class BenefitDocument
		{
			[NotEmpty]
			public Guid guid { get; set; }

			[NotEmpty]
			public Guid hubguid { get; set; }

			[NotEmpty]
			public Guid personguid { get; set; }

			[NotEmpty]
			public string aggregator { get; set; }

			[NotEmpty]
			public List<Family> family { get; set; }

			[NotEmpty]
			public string file { get; set; }

			[NotEmpty]
			public string filetype { get; set; }
		}
	}
}