using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace ProvidersAppservice.Models
{
    public class PriceTableCB 
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string providerproductcode { get; set; }
        public string aggregator { get; set; }
        public List<PriceTable> prices { get; set; }
    }
}
