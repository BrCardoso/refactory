using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;

using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interfaces;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Commons.Models;

namespace NetCoreJobsMicroservice.Repository
{
	public class EventsRepository : IEventsRepository
	{
		private readonly IBucket _bucket;

		public EventsRepository(IBucketProvider bucketProvider, IOptions<List<BucketName>> buckets)
		{
			_bucket = bucketProvider.GetBucket(buckets.Value[0].Name);
		}

		public async Task<Event> FindAsync(Guid hubguid, string aggregator)
		{
			var query = new QueryRequest()
				.Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = 'Events' AND d.hubguid = $hubguid AND d.aggregator = $aggregator;")
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$aggregator", aggregator)
				.Metrics(false);

			var result = await _bucket.QueryAsync<Event>(query);

			return result.SingleOrDefault();
		}

		public async Task<Event> UpSertAsync(Event newEvent)
		{
			var result = await _bucket.UpsertAsync(newEvent.guid.ToString(), newEvent);
			return result.Value;
		}
	}
}