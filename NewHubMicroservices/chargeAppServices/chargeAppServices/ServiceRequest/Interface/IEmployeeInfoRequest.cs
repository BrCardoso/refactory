using chargeAppServices.Models;
using Commons.Base;
using System;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest.Interface
{
    public interface IEmployeeInfoRequest
    {
        Task<EmployeesModel> Get(Guid hubguid, string aggregator, string authorization);
    }
}
