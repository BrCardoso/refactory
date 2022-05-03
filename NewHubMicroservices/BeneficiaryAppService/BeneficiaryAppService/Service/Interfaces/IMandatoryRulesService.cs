using BeneficiaryAppService.Models;
using Commons;
using Commons.Base;
using System;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface IMandatoryRulesService
	{
		Task<MethodFeedback> Validate(Guid hubguid, string aggregator, Benefitinfo beneficio, PersonDB person, string kinship, string typeuser, Guid employeeGuid, string authorization, bool notify);
	}
}
