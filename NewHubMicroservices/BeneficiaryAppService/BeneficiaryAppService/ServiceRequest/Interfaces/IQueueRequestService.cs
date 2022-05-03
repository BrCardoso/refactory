using BeneficiaryAppService.Models;
using Commons;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface IQueueRequestService
    {
        Task<MethodFeedback> Post(QueueModel itemArq, string authorization);
    }
}
