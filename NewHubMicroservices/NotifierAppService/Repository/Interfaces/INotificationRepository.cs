using NotifierAppService.Models;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NotifierAppService.Repository.Interfaces
{
	public interface INotificationRepository
	{
		Task<NotifierDB> FindByAggregatorAsync(Guid hubguid, string aggregator);

		Task<IEnumerable<NotifierData>> FindAllNotificationsByAggregatorAsync(string aggregator, string hubGuid);

		Task<NotifierDB> UpsertAsync(NotifierDB notifierData);

	}
}