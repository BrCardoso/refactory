using Commons.Base;
using System;
using System.Collections.Generic;

namespace BIAppService.Model.Request
{
    public class InsuranceClaimRequest
    {
        public Guid guid { get; set; }

        public Guid hubguid { get; set; }

        public string aggregator { get; set; }

        public List<Insuranceclaim> Insuranceclaims { get; set; }

    }
}
