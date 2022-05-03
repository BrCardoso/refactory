using NotifierAppService.Models;
using System;
using System.Threading.Tasks;

namespace NotifierAppService.Services.Interfaces
{
	public interface INotificationService
	{
		Task<NotificationStatus> AddNotificationAsync(Guid hubguid, string aggregator, NotifierData notifier);
	}
}