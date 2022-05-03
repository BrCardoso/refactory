using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuBdoRH.NIT.Services.Interface
{
    public interface IChargeService
    {
        Task<bool> PostUpdateCharge(Guid chargeGuid, Commons.Base.Nit.NitModel nitTask, string aggregator, string authorization);
    }
}
