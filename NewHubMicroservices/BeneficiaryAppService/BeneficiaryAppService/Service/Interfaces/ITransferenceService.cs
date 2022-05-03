using BeneficiaryAppService.Models;
using Commons;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface ITransferenceService
	{
		Task<MethodFeedback> ValidateAsync(QueueModel model, string authorization);
	}
}
