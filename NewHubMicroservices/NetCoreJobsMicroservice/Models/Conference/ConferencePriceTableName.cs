using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
	public class ConferencePriceTableName
	{
		public class Product
		{
			public string code { get; set; }
			public string productpricetablename { get; set; }
		}

		public Guid guid { get; set; }
		public Guid hubguid { get; set; }
		public Guid providerguid { get; set; }
		public string aggregator { get; set; }
		public string contractnumber { get; set; }
		public List<Product> products { get; set; }
	}
}