using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace LoginAppService.Model
{
    public class HubCustomerSearch : MethodFeedback
    { 
        public string Status { get; set; }
        public string companyid { get; set; }
        public string companyName { get; set; }
        public Guid companyguid { get; set; }
        public string aggregator { get; set; }
        public object groups { get; set; }
    }
}
