using System.Collections.Generic;

namespace chargeAppServices.Models
{
    public class Holidays
    {
        public List<Dates> Holiday { get; set; }

        public class Dates
        {
            public string Data { get; set; }
            public string Descricao { get; set; }
        }
    }
}