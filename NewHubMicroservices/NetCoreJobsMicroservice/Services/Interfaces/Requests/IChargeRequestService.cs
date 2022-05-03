using NetCoreJobsMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface IChargeRequestService
    {
        Task<bool> UpdateAsync(List<ChargeOrder> chargeOrder, string authorization);
        Task<ChargeOrder> getAsync(Guid token, Guid id, string aggregator, string authorization);
    }
}
