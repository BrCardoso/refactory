using Commons.Base;
using System;
using System.Collections.Generic;

namespace BIAppService.Model.Request
{
    public class HubCustomerRequest
    {
        public Guid guid { get; set; }
        public string contractNumber { get; set; }
        public string contractIssued { get; set; }
        public string status { get; set; }
        public string blockdate { get; set; }
        public string blockreason { get; set; }
        public Hierarchy hierarchy { get; set; }
        public List<CompanyStruCB> companies { get; set; }

        public class CompanyStruCB
        {
            public Guid companyguid { get; set; }
            public string aggregator { get; set; }
            public string GroupName { get; set; }
            public string branchName { get; set; }
        }
    }
}
