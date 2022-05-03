using Commons.Models;

using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository.Interfaces
{
	public interface IEventsRepository
	{
		Task<Event> FindAsync(Guid hubguid, string aggregator);

		Task<Event> UpSertAsync(Event newEvent);
	}
}