using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuBCustomerAppService.Models.Response
{
    public class IdentityVal
    {
        public string Status { get; set; }
        public string companyid { get; set; }
        public string companyName { get; set; }
        public Guid companyguid { get; set; }
        public string aggregator { get; set; }
        public object groups { get; set; }
    }
}
