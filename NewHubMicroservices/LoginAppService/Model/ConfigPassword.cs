using System;
using System.Collections.Generic;

namespace LoginAppService.Model
{
	public class ConfigPassword
	{
		public class Data
		{
			public Guid requestguid { get; set; }
			public Guid userguid { get; set; }
			public DateTime requestDateTime { get; set; }
			public DateTime? usedDateTime { get; set; }
		}

		public string docType { get; set; } = "ConfigPassword";
		public List<Data> requests { get; set; }
		public Guid guid { get; set; }
	}
}