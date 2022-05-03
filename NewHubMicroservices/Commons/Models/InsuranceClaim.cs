using System;

namespace Commons.Base
{
    public class Insuranceclaim
    {
        public string Providercnpj { get; set; }
        public string Refdate { get; set; }
        public string Cardnumber { get; set; }
        public string Name { get; set; }
        public DateTime Startdate { get; set; }
        public DateTime? Finaldate { get; set; }
        public string Type { get; set; }
        public string Cid { get; set; }
        public string Ciddesc { get; set; }
        public string Procedure { get; set; }
        public string Serviceprovider { get; set; }
        public string Risk { get; set; }
        public DateTime CreationDate { get; set; }
    }
}
