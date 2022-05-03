using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using Commons;
using Commons.Base;
using Commons.Enums;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface IRulesConfigurationService
	{
		Task<MethodFeedback> Validate(Guid hubguid, string aggregator, MovimentTypeEnum tipo, PersonDB person, Benefitinfo benefit, EmployeeInfo employee, string inputkinship, string authorization);
		Dictionary<string, object> ValidateRuleConf(Guid hubguid, string aggregator, MovimentTypeEnum tipo, PersonDB person, Benefitinfo benefit, EmployeeInfo employee, string inputkinship, RulesConfigurationModel RC);
	}
}
