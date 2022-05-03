using Commons.Base;
using System;

namespace NetCoreJobsMicroservice.Models.Base.Output
{
    public class LogCB : Log
    {
        public Guid guid { get; set; }
        public Guid hubguid { get; set; }
        public DateTime start { get; set; }
        public DateTime? end { get; set; }
    }
}
