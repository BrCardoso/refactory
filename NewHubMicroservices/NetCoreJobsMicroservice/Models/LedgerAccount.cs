using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models
{
    public class STATEMENT
    {
        public string INVOICEVALIDATIONGUID { get; set; }
        public DateTime? DATE { get; set; }
        public float? VALUE { get; set; }
        public string TYPE { get; set; }
    }

    public class ACCOUNT
    {
        public string COMPANYID { get; set; }
        public string BRANCH { get; set; }
        public Guid PROVIDERGUID { get; set; }
        public float? CURRENTVALUE { get; set; }
        public List<STATEMENT> STATEMENT { get; set; }
    }

    public class LedgerAccountModel
    {
        public Guid GUID { get; set; }
        public Guid HUBGUID { get; set; }
        public ACCOUNT ACCOUNT { get; set; }
    }
}