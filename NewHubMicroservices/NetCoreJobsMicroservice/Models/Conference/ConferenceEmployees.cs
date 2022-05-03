using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
	public class ConferenceEmploees
	{
		public class EmployeeData
		{
			public Guid personguid { get; set; }
			public Guid familyguid { get; set; }
			public double? salary { get; set; }
		}

		public Guid guid { get; set; }
		public Guid hubguid { get; set; }
		public string aggregator { get; set; }
		public List<EmployeeData> employees { get; set; }
	}
}