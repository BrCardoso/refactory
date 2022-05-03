using NotifierAppService.Data.RPC.Clients.Interfaces;
using NotifierAppService.Hubs;
using NotifierAppService.Models;
using NotifierAppService.Repository.Interfaces;
using NotifierAppService.Services.Interfaces;

using Microsoft.AspNetCore.SignalR;

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace NotifierAppService.Services
{
	public class NotificationService : INotificationService
	{
		private readonly IHubContext<NotificationHub, INotificationClient> _hubContext;
		private readonly INotificationRepository _notificationRepository;

		public NotificationService(IHubContext<NotificationHub, INotificationClient> hubContext, INotificationRepository notificationRepository)
		{
			_hubContext = hubContext;
			_notificationRepository = notificationRepository;
		}

		public async Task<NotificationStatus> AddNotificationAsync(Guid hubguid, string aggregator, NotifierData newNotification)
		{
			NotifierDB currentNotifier = await _notificationRepository.FindByAggregatorAsync(hubguid, aggregator);
			if (currentNotifier is null)
				currentNotifier = new NotifierDB
				{
					guid = Guid.NewGuid(),
					aggregator = aggregator,
					docType = "Notifier",
					hubguid = hubguid,
					Notifications = new List<NotifierData>()
				};

			newNotification.guid = Guid.NewGuid();
			newNotification.dateTime = DateTime.Now;
			newNotification.read = false;
			currentNotifier.Notifications.Add(newNotification);

			//TODO: Validar se aggregator e hubguid sao valores validos de alguma empresa

			if (!(await _notificationRepository.UpsertAsync(currentNotifier) is NotifierDB))
				return NotificationStatus.ErrorOnSave;

			await _hubContext.Clients.Group($"{aggregator}:{hubguid}").NewNotificationAsync(newNotification);
			return NotificationStatus.Success;
		}
	}

	public enum NotificationStatus
	{
		[Description("Notificaçao adicionada e enviada com sucesso!")]
		Success,

		[Description("Nao foi possivel guardar a notificaçao no banco de dados!")]
		ErrorOnSave,

		[Description("É necessario informar o 'aggregator' e 'hubGuid'")]
		EmptyOrNullFields
	}
}