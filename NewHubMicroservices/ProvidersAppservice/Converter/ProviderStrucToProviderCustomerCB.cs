using ProvidersAppservice.Models;
using System.Collections.Generic;

namespace ProvidersAppservice.Converter
{
    public static class ProviderStrucToProviderCustomerCB
    {
        public static ProviderCustomerCB Convert(ProviderStrucCB v)
        {
            ProviderCustomerCB ret = new ProviderCustomerCB
            {
                Guid = v.Guid,
                Hubguid = v.Hubguid,
                Providerguid = v.Providerguid,
                Aggregator = v.Aggregator,
                Code = v.Code,
                Emailinfos = v.Emailinfos,
                accesscredentials = v.accesscredentials,
                Products = new List<ProviderCustomerCB.ProductStruct>()
            };
            foreach (var item in v.Products)
            {
                var ben = new ProviderCustomerCB.ProductStruct
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
