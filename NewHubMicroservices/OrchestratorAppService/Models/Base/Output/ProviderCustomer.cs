using System;
using System.Collections.Generic;
using Commons;
using Commons.Base;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class ProviderCustomerOut
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string aggregator { get; set; }
        public string code { get; set; }
        public string clientBlockreason { get; set; }
        public DateTime? clientBlockdate { get; set; }
        public List<Emailinfo> emailinfos { get; set; }
        public List<ProductOut> products { get; set; }
        public Accesscredentials accesscredentials { get; set; }

        public partial class ProductOut
        {
            private string _Code;
            public string Code
            {
                get { return _Code; }
                set { _Code = string.IsNullOrEmpty(value) ? "null" : value; }
            }

            public string providerproductcode { get; set; }
            public string ClientBlockreason { get; set; }
            public DateTime? ClientBlockdate { get; set; }

            public static explicit operator ProductOut(ProductStruc v)
            {
                ProductOut ret = new ProductOut
                {
                    Code = v.Code,
                    providerproductcode = v.Providerproductcode,
                    ClientBlockdate = v.clientBlockdate,
                    ClientBlockreason = v.clientBlockreason
                };

                return ret;
            }
        }

        public static explicit operator ProviderCustomerOut(ProviderStruc v)
        {
            ProviderCustomerOut providerCustomerOut = new ProviderCustomerOut
            {
                accesscredentials = v.accesscredentials,
                clientBlockdate = v.clientBlockdate,
                clientBlockreason = v.clientBlockreason,
                code = v.Code,
                emailinfos = v.emailinfos,
                products = new List<ProductOut>()

            };
            providerCustomerOut.accesscredentials = v.accesscredentials;
            if (v.Products != null)
            {
                foreach (var prod in v.Products)
                {
                    providerCustomerOut.products.Add((ProductOut)prod);
                }
            }

            return providerCustomerOut;
        }
    }
}
