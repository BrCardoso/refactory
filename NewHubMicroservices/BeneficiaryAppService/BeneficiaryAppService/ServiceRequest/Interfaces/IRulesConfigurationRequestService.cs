using BeneficiaryAppService.Models.External;
using System;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface IRulesConfigurationRequestService
    {
        Task<RulesConfigurationModel> Get(Guid hubguid, string aggregator, Guid providerguid, string contractNumber, string authorization);
    }
}
