using Commons;
using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace ProvidersAppservice.Models
{
    public class ProviderCustomerCB : ProviderClean
    {
        public Guid Guid { get; set; }
        [NotEmpty]
        public Guid Hubguid { get; set; }
        [NotEmpty]
        public Guid Providerguid { get; set; }
        [NotEmpty]
        public string Aggregator { get; set; }
        [NotEmpty]
        public string Code { get; set; }
        public List<Emailinfo> Emailinfos { get; set; }
        public List<ProductStruct> Products { get; set; }
        public Accesscredentials accesscredentials { get; set; }

        public class ProductStruct : ProviderStrucCB.ProductCB
        {
            public string Description { get; set; }
        }
    }
    
    public class ProviderStrucCB
    {
        public Guid Guid { get; set; }
        [NotEmpty]
        public Guid Hubguid { get; set; }
        [NotEmpty]
        public Guid Providerguid { get; set; }
        [NotEmpty]
        public string Aggregator { get; set; }
        [NotEmpty]
        public string Code { get; set; }
        public List<Emailinfo> Emailinfos { get; set; }
        public List<ProductCB> Products { get; set; }
        public Accesscredentials accesscredentials { get; set; }

        public partial class ProductCB
        {
            [NotEmpty]
            public string Code { get; set; }
            [NotEmpty]
            public string Providerproductcode { get; set; }
            public string ClientBlockreason { get; set; }
            public bool? copart { get; set; }
            public string ClientBlockdate { get; set; }
        }

    }

}
