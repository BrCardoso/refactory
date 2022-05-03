using NotifierAppService.Models;

using System.Threading.Tasks;

namespace NotifierAppService.Data.RPC.Clients.Interfaces
{
	public interface INotificationClient
	{
		Task NewNotificationAsync(NotifierData newNotification);

		Task InputNotValidAsync(string message);

		Task NotificationNotAddedAsync(string message);

		Task NotificationMarkedAsReadAsync(NotifierData notifier);

		Task NotificationNotMarkedAsReadAsync(string message);

		Task SuccessfullyJoinedAsync(string message);

		Task SuccessfullyExitedAsync(string message);
	}
}