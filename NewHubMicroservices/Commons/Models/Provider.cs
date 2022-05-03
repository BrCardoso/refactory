using System.Collections.Generic;

namespace Commons.Base
{
    /// <summary>
    /// objeto com as informaçoes basicas
    /// </summary>
    public class Provider : ProviderClean
    {
        public List<Product> Products { get; set; }

        public static explicit operator Provider(ProviderStruc v)
        {
            Provider ret = new Provider {
                Cnpj = v.Cnpj,
                Description = v.Description,
                Products = new List<Product>(),
                Registration = v.Registration,
                Segcode = v.Segcode,
                Site = v.Site,
                Status = v.Status
            };
            if (v.Products != null)
            {
                foreach (var item in v.Products)
                {
                    Product pd = new Product
                    {
                        Complementaryinfo = item.Complementaryinfo,
                        Description = item.Description,
                        Providerproductcode = item.Providerproductcode,
                        Subsegcode = item.Subsegcode
                    };
                    ret.Products.Add(pd);
                }
            }
            
            return ret;


        }        
    }
    public class ProviderClean
    {
        public string Cnpj { get; set; }
        public string Registration { get; set; }
        public string Description { get; set; }

        private string _Segcode;
        public string Segcode
        {
            get { return _Segcode?.ToUpper(); }
            set { _Segcode = value?.ToUpper(); }
        }
        public string Site { get; set; }
        public string Status { get; set; }
    }

    public class Product
    {
        public string Providerproductcode { get; set; }
        public string Description { get; set; }

        private string _Subsegcode;
        public string Subsegcode
        {
            get { return _Subsegcode?.ToUpper(); }
            set { _Subsegcode = value?.ToUpper(); }
        }
        public bool? copart { get; set; }
        public List<Complementaryinfo> Complementaryinfo { get; set; }
        public List<PriceTable> PriceTable { get; set; }
    }
}
