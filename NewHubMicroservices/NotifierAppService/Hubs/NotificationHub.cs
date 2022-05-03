using NotifierAppService.Data.RPC.Clients.Interfaces;
using NotifierAppService.Models;
using NotifierAppService.Repository.Interfaces;

using Microsoft.AspNetCore.SignalR;

using System;
using System.Threading.Tasks;
using System.Linq;

namespace NotifierAppService.Hubs
{
	public class NotificationHub : Hub<INotificationClient>
	{
		private readonly INotificationRepository _notificationRepository;

		public NotificationHub(INotificationRepository notificationRepository)
		{
			_notificationRepository = notificationRepository;
		}

		public async Task JoinAtGroupAsync(string aggregator, string hubGuid)
		{
			if (string.IsNullOrEmpty(aggregator) || string.IsNullOrEmpty(hubGuid))
			{
				await Clients.Caller.InputNotValidAsync("É necessario informar o 'aggregator' e 'hubGuid'");
				return;
			}

			await Groups.AddToGroupAsync(Context.ConnectionId, $"{aggregator}:{hubGuid}");
			await Clients.Caller.SuccessfullyJoinedAsync("Agora voce receberá as notificaçoes!");
		}

		public async Task LeaveAtGroupAsync(string aggregator, string hubGuid)
		{
			if (string.IsNullOrEmpty(aggregator) || string.IsNullOrEmpty(hubGuid))
			{
				await Clients.Caller.InputNotValidAsync("É necessario informar o 'aggregator' e 'hubGuid'");
				return;
			}

			await Clients.Caller.SuccessfullyExitedAsync("Voce nao receberá mais as notificaçoes!");
			await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"{aggregator}:{hubGuid}");
		}

		public async Task MarkNotificationAsReadAsync(string aggregator, Guid hubGuid, Guid guid)
		{
			if (string.IsNullOrEmpty(aggregator) || hubGuid == Guid.Empty || guid == Guid.Empty)
			{
				await Clients.Caller.InputNotValidAsync("É necessario informar o 'aggregator' e 'hubGuid' e 'guid'");
				return;
			}

			if (!(await _notificationRepository.FindByAggregatorAsync(hubGuid, aggregator) is NotifierDB currentNotifier))
			{
				await Clients.Caller.NotificationNotMarkedAsReadAsync("Nao foi possivel marcar a notificaçao como lida, tente novamente!");
				return;
			}

			if (!(currentNotifier.Notifications.SingleOrDefault(x => x.guid == guid) is NotifierData notification))
			{
				await Clients.Caller.NotificationNotMarkedAsReadAsync("Nao foi possivel marcar a notificaçao como lida, tente novamente!");
				return;
			}

			notification.read = true;

			if (!(await _notificationRepository.UpsertAsync(currentNotifier) is NotifierDB updatedNotification))
			{
				await Clients.Caller.NotificationNotMarkedAsReadAsync("Nao foi possivel marcar a notificaçao como lida, tente novamente!");
				return;
			}

			await Clients.Group($"{aggregator}:{hubGuid}").NotificationMarkedAsReadAsync(notification);
		}
	}
}