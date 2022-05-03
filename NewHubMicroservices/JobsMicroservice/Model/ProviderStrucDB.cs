using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;

namespace JobsMicroservice.Model
{

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
}