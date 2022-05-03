using ProvidersAppservice.Models;
using System.Collections.Generic;

namespace ProvidersAppservice.Converter
{
    public static class ProviderCustomerCBToProviderStrucCB
    {
        public static ProviderStrucCB Convert(ProviderCustomerCB v)
        {
            ProviderStrucCB ret = new ProviderStrucCB
            {
                Guid = v.Guid,
                Hubguid = v.Hubguid,
                Providerguid = v.Providerguid,
                Aggregator = v.Aggregator,
                Code = v.Code,
                Emailinfos = v.Emailinfos,
                accesscredentials = v.accesscredentials,
                Products = new List<ProviderStrucCB.ProductCB>()
            };
            foreach (var item in v.Products)
            {
                var ben = new ProviderStrucCB.ProductCB
                {
                    copart = item.copart,
                    Code = item.Code,
                    ClientBlockdate = item.ClientBlockdate,
                    ClientBlockreason = item.ClientBlockreason,
                    Providerproductcode = item.Providerproductcode
                };

                ret.Products.Add(ben);
            }
            return ret;
        }
    }
}
