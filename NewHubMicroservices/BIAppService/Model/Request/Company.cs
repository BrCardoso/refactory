using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BIAppService.Model.Request
{
    public class Company : Commons.Base.Company
    {
        public Guid guid { get; set; }

    }
}
