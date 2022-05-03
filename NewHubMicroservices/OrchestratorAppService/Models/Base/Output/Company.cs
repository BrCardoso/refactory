using Commons.Base;
using System;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class CompanyOut : Company
    {
        public Guid guid { get; set; }

        public static explicit operator CompanyOut(CompanyStruc v)
        {
            CompanyOut ret = new CompanyOut
            {
                Addresses = v.Addresses,
                Branchname = v.Branchname,
                companyid = v.companyid,
                Companyname = v.Companyname,
                Complementaryinfos = v.Complementaryinfos,
                Description = v.Description,
                TradingName = v.TradingName
            };

            return ret;
        }
    }
}
