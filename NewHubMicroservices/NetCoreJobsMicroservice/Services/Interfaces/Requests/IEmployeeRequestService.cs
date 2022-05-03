using Commons.Base;
using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface IEmployeeRequestService
    {
        Task<Employeeinfo> getEmployeeInfoAsync(Guid employeeguid, Guid hubguid, string aggregator, string authorization);
    }
}
