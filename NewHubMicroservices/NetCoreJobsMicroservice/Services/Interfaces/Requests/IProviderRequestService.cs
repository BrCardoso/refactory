using Commons;
using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface IProviderRequestService
    {
        Task<ProviderDB> getProviderAsync(Guid guid);
        Task<string> findPriceTableAsync(Guid token, Guid providerguid, string code, string aggregator, string authorization);
        Task<ProviderStrucDB> getProviderStrucAsync(Guid hubguid, Guid providerguid, string aggregator, string authorization);
       
    }
}
