using System;
using System.Collections.Generic;
using Commons;
using System.Linq;
using Commons.Base;

namespace BeneficiaryAppService.Models
{

    public class PersonDocs
    {
        public Guid personguid { get; set; }
        public List<Document> documents { get; set; }
    }

    public class PersonComplementaryinfos
    {
        public Guid personguid { get; set; }
        public List<Complementaryinfo> complementaryinfos { get; set; }
    }
}