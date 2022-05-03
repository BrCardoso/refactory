using BIAppService.Model.Request;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BIAppService.ServiceRequest.Interface
{
    public interface IServiceRequests
    {
        Task<HubCustomerRequest> GetHubCustomerAsync(Guid hubguid, string authorization);
        Task<InsuranceClaimRequest> GetInsuranceClaimAsync(Guid hubguid, string authorization);
        Task<List<Family>> GetBeneficiariesAsync(Guid token, string authorization);
        Task<Provider> GetProviderAsync(Guid providerguid, string authorization);
        Task<Company> GetCompanyAsync(string companyguid, string authorization);
    }
}
