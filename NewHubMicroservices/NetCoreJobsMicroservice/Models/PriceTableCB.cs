using Commons.Base;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public class PriceTableCB
    {
        public Guid guid { get; set; }
        public Guid Hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string providerproductcode { get; set; }
        public string aggregator { get; set; }
        public List<PriceTable> prices { get; set; }
    }
}