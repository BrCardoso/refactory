using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface IHubCustomerRequestService
    {
        Task<HubCustomerOut> getHubCustomerAsync(Guid token);
    }
}
