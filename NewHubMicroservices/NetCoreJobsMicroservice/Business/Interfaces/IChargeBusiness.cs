using Commons;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Business.Interfaces
{
    public interface IChargeBusiness
    {
        Task<List<MethodFeedback>> HandleChargeAsync(Guid token, Guid id, string aggregator, string authorization);
    }
}
