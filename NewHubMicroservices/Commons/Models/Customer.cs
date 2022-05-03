using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Base
{
    public class CustomerFull
    {
        [NotEmpty]
        public Guid guid { get; set; }
        public string contractNumber { get; set; }
        public string contractIssued { get; set; }
        public string status { get; set; }
        public string blockdate { get; set; }
        public string blockreason { get; set; }
        public List<Hierarchy> Hierarchy { get; set; }
        public List<CompanyStruc> Companies { get; set; }

        //public CustomerFull()
        //{
        //    this.Companies = new List<CompanyStruc> {new CompanyStruc() };
        //    this.Hierarchy = new List<Hierarchy> {new Hierarchy() };
        //}
    }
}
