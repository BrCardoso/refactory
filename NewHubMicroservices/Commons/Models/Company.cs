using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Base
{   
    public class Company
    {
        [NotEmpty]
        public string companyid { get; set; }
        public string Companyname { get; set; }
        public string TradingName { get; set; }
        public string Branchname { get; set; }
        public string Description { get; set; }
        public List<Address> Addresses { get; set; }
        public List<Complementaryinfo> Complementaryinfos { get; set; }

        //public Company()
        //{
        //    this.Addresses = new List<Address> { new Address()};
        //    this.Complementaryinfos = new List<Complementaryinfo> { new Complementaryinfo()};
        //}
    }

}
