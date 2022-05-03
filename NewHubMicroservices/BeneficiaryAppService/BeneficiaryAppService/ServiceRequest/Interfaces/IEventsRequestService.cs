using BeneficiaryAppService.Models;
using Commons.Models;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest.Interfaces
{
    public interface IEventRequestService
	{
		Task<Event> CreateEventAsync(Event newEvent, string authorization);
	}
}
