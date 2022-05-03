using BeneficiaryAppService.Models;
using System;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface IProviderRequestService
    {
        Task<ProviderStrucDB> GetProviderStruc(Guid hubguid, string aggregator, Guid providerguid, string authorization);

        Task<ProviderDB> GetProvider(Guid providerguid, string productcode);
        
    }
}