using chargeAppServices.Models;
using System;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest.Interface
{
    public interface IHubCustomerRequest
    {
        Task<CustomerFull.HuBCustomerModel> GetAsync(Guid guid);
    }
}
