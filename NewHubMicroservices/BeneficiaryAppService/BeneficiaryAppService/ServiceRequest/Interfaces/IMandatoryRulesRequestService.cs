using Commons.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface IMandatoryRulesRequestService
    {
        Task<List<MandatoryRules>> getRules(Guid provider);
    }
}
