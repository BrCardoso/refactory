using System;
using System.Collections.Generic;
using Commons;
using Commons.Base;

namespace BeneficiaryAppService.Models
{
    public class EmployeeDB  {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public string aggregator { get; set; }
        public List<EmployeeInfo> employees { get; set; }

    }
       
    public class ProviderStrucDB
    {
        public Guid Guid { get; set; }
        public Guid Hubguid { get; set; }
        public Guid Providerguid { get; set; }
        public string Aggregator { get; set; }
        public string Code { get; set; }
        public string ClientBlockreason { get; set; }
        public DateTime? ClientBlockdate { get; set; }
        public List<Emailinfo> Emailinfos { get; set; }
        public List<ProductCB> Products { get; set; }
        public Accesscredentials accesscredentials { get; set; }

        public partial class ProductCB
        {
            public string Code { get; set; }
            public string Providerproductcode { get; set; }
            public string ClientBlockreason { get; set; }
            public string ClientBlockdate { get; set; }
        }

    }

    public class ProviderDB : Provider
    {
        public Guid guid { get; set; }
    }
}