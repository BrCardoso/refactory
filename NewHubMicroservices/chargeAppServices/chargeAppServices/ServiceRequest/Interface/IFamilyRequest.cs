using chargeAppServices.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest.Interface
{
    public interface IFamilyRequest
    {
        Task<List<Family>> Get(Guid hubguid, string aggregator, string authorization);
    }
}
