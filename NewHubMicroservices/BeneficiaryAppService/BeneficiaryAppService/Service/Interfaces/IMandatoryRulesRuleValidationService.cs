using BeneficiaryAppService.Models;
using Commons.Base;
using Commons.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface IMandatoryRulesRuleValidationService
    {
        Task<Dictionary<string, object>> ValidateAsync(MandatoryRules rule, Guid hubguid, string aggregator, Benefitinfo beneficio, PersonDB person, string kinship, string typeuser, Guid employeeGuid, string authorization);
    }
}
