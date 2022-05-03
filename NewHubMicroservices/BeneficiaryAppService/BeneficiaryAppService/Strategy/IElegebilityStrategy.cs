
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Strategy
{
    public interface IElegebilityStrategy
    {
        string Validate(EmployeeInfo employee, bool elegBoll, Dictionary<string, object> dictEleg, productruleHealth prodRuleConfig);
    }
}
