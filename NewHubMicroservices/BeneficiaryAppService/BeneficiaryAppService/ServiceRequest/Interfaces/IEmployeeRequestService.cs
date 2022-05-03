using BeneficiaryAppService.Models;
using System;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface IEmployeeRequestService
    {
        Task<EmployeeInfo> getInfo(Guid hubguid, string aggregator, Guid employeeguid, string authorization);
        Task<EmployeeDB> getInfoByPersonGuid(Guid hubguid, string aggregator, Guid employeeguid, string authorization);
        Task<EmployeeDB> getInfoByRegistration(Guid hubguid, string aggregator, string registration, string authorization);
        Task<EmployeeModel> Post(Guid hubguid, string aggregator, EmployeeInfo employeeInfo,  string authorization);
       
    }
}