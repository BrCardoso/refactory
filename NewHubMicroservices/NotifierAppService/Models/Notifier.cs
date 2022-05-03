using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace NotifierAppService.Models
{
	public class NotifierData
	{
		[Required]
		public string type { get; set; }

		public bool read { get; set; }

		[Required]
		public string title { get; set; }

		[Required]
		public string description { get; set; }

		public DateTime dateTime { get; set; }
		public Guid guid { get; set; }
	}

	public class NotifierDB
	{
		public Guid guid { get; set; }
		public Guid hubguid { get; set; }
		public string docType { get; set; }
		public string aggregator { get; set; }
		public List<NotifierData> Notifications { get; set; }
	}
}