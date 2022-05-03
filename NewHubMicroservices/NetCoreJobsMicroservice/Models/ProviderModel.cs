using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public class COMPLEMENTARYINFO
    {
        public string TYPE { get; set; }
        public string VALUE { get; set; }
    }

    public class PRODUCT
    {
        public string CODE { get; set; }
        public string PROVIDERPRODUCTCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public List<COMPLEMENTARYINFO> COMPLEMENTARYINFO { get; set; }
    }

    public class ACCESSCREDENTIALS
    {
        public string LOGIN { get; set; }
        public string PASSWORD { get; set; }
        public string COSTUMERNUMBER { get; set; }
    }

    public class PROVIDERModel
    {
        public string GUID { get; set; }
        public string SEGCODE { get; set; }
        public string DESCRIPTION { get; set; }
        public string CNPJ { get; set; }
        public string EMAIL { get; set; }
        public string SITE { get; set; }
        public string STATUS { get; set; }
        public List<PRODUCT> PRODUCT { get; set; }
    }
}