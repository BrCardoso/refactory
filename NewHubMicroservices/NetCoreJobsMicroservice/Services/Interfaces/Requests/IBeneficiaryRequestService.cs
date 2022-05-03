using Commons;
using Commons.Base;
using NetCoreJobsMicroservice.Models;
using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface IBeneficiaryRequestService
    {
        Task<Person> getPersonAsync(Guid guid);
        Task<MethodFeedback> PostFamilyAsync(TaskReturnModel itemArq, string authorization);
    }
}
