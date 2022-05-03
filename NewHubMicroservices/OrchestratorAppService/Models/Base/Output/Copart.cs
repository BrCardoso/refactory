using Commons.Base;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class CopartOut
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public Guid providerguid { get; set; }
        public string aggregator { get; set; }
        public List<Copart> Coparticipations { get; set; }

        public static explicit operator CopartOut(List<Copart> v)
        {
            CopartOut copartOut = new CopartOut
            {
                Coparticipations = v
            };
            return copartOut;
        }
    }
}
