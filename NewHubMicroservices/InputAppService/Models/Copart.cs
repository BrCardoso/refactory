using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace InputAppService.Models
{
    public class CopartModel
    {
        public Guid guid { get; set; }

        [NotEmpty]
        public Guid hubguid { get; set; }

        [NotEmpty]
        public Guid providerguid { get; set; }

        [NotEmpty]
        public string aggregator { get; set; }

        public List<Coparticipation> Coparticipations { get; set; }
    }
    public class Coparticipation : Commons.Base.Copart
    {
        public DateTime CreationDate { get; set; }
    }
}
