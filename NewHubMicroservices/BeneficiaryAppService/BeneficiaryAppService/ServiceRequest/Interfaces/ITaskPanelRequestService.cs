using BeneficiaryAppService.Models;
using Commons;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface ITaskPanelRequestService
    {
        Task<MethodFeedback> Post(TaskPanelModel itemArq, bool notify, string authorization);
    }
}
