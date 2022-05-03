using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace InputAppService.Models
{
    public class EntityModel
    {
        public Guid guid { get; set; }

        [NotEmpty]
        public Guid hubguid { get; set; }

        [NotEmpty]
        public Guid providerguid { get; set; }

        [NotEmpty]
        public string aggregator { get; set; }

        public List<Commons.Base.Entity> entities { get; set; }
    }
}
