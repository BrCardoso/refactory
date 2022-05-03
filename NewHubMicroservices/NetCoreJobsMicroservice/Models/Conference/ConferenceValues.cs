using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
	public class ConferenceValues
	{
		public string type { get; set; }
		public string name { get; set; }
		public DateTime creationdate { get; set; }
		public List<Range> range { get; set; }

		public class Range

		{

			public double? initialrange { get; set; }
			public double? finalrange { get; set; }
			public double employeevalue { get; set; }
			public double relativevalue { get; set; }
			public double householdvalue { get; set; }
			public string discounttype { get; set; }
			public double employeediscountvalue { get; set; }
			public double relativediscountvalue { get; set; }
			public double householddiscountvalue { get; set; }

		}
	}
}