using System;
using System.Collections.Generic;


namespace Commons.Base
{
    /// <summary>
    /// objeto com as informaçoes basicas
    /// </summary>
    public class ProviderStruc : ProviderClean
    {
        public string Code { get; set; }
        public string clientBlockreason { get; set; }
        public DateTime? clientBlockdate { get; set; }
        public List<Emailinfo> emailinfos { get; set; }
        public List<ProductStruc> Products { get; set; }
        public Accesscredentials accesscredentials { get; set; }
        //public ProviderStruc()
        //{
        //    this.Products = new List<ProductStruc> { new ProductStruc() };
        //    this.emailinfos = new List<Emailinfo> { new Emailinfo() };
        //    this.Accesscredentials = new Accesscredentials();
        //}
    }

    public class ProductStruc : Product
    {
        public string Code { get; set; }
        public string clientBlockreason { get; set; }
        public DateTime? clientBlockdate { get; set; }
        public List<PriceTable> PriceTable { get; set; }

        //public ProductStruc()
        //{
        //    this.PriceTable = new PriceTable();
        //}
    }

    public partial class Accesscredentials
    {
        public string Login { get; set; }
        public string Password { get; set; }
        public string Costumernumber { get; set; }
    }
}
