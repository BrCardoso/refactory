using BeneficiaryAppService.Models;
using Commons;
using Commons.Base;
using Commons.Enums;
using System;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface INITService
    {
        Task<MethodFeedback> SendAsync(Guid hubguid, string aggregator, Benefitinfo beneficio, MovimentTypeEnum typeMovement, PersonDB findPerson, PersonDB findEmployee, EmployeeInfo findEmployeeInfo,
            string beneficiarioTpUser, string beneficiarioKinship, string authorization);

    }
}
