using System;

using Commons;

namespace BeneficiaryAppService.Models
{
    public class CardReissue : MethodFeedback
    {
		public Guid guid { get; set; }

		public Guid hubguid { get; set; }

		public Guid familyguid { get; set; }

		public Guid personguid { get; set; }

		public string personName { get; set; }

		public Guid providerguid { get; set; }

		public string providerName { get; set; }

		public string aggregator { get; set; }

		public string providerproductcode { get; set; }

		public string product { get; set; }

		public string reissuereason { get; set; }

		public string reissuedocument { get; set; }
	}
}