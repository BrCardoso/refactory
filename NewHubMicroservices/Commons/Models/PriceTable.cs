using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Base
{
    public class PriceTable
    {
        [NotEmpty]
        public string Type { get; set; }
        [NotEmpty]
        public string Name { get; set; }
        public DateTime creationdate { get; set; }
        [NotEmpty]
        public List<Ranges> Range { get; set; }

        public partial class Ranges
        {
            public float? Initialrange { get; set; }
            public float? Finalrange { get; set; }
            public float Employeevalue { get; set; }
            public float Relativevalue { get; set; }
            public float Householdvalue { get; set; }
            public string Discounttype { get; set; }
            public float Employeediscountvalue { get; set; }
            public float Relativediscountvalue { get; set; }
            public float Householddiscountvalue { get; set; }
        }

    }
}
