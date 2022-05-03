using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace InputAppService.Models
{
    public class InsuranceClaimModel
    {
        public Guid guid { get; set; }

        [NotEmpty]
        public Guid hubguid { get; set; }

        [NotEmpty]
        public string aggregator { get; set; }

        public List<Insuranceclaims> Insuranceclaims { get; set; }
    }
    public class Insuranceclaims : Commons.Base.Insuranceclaim
    {
        public DateTime CreationDate { get; set; }
    }
}
