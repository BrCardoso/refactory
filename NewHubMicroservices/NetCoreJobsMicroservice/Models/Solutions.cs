using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
	public class WarningSolution
	{
		public string Description { get; set; }
		public List<string> solutions { get; set; }
	}

	public class Solutions
	{
		public List<WarningSolution> Warnings { get; set; }
	}
}