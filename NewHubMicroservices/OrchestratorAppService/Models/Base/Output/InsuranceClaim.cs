using Commons.Base;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class InsuranceclaimOut
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public string aggregator { get; set; }
        public List<Insuranceclaim> Insuranceclaims { get; set; }

        public static explicit operator InsuranceclaimOut(List<Insuranceclaim> v)
        {
            InsuranceclaimOut insuranceclaimOut = new InsuranceclaimOut
            {
                Insuranceclaims = v
            };
            return insuranceclaimOut;
        }
    }
}
