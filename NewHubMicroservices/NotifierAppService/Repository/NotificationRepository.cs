using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;

using NotifierAppService.Models;
using NotifierAppService.Repository.Interfaces;

using Microsoft.Extensions.Options;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NotifierAppService.Repository
{
	public class NotificationRepository : INotificationRepository
	{
		private readonly IBucket _bucket;

		public NotificationRepository(IBucketProvider bucketProvider, IOptions<List<BucketName>> options)
		{
			_bucket = bucketProvider.GetBucket(options.Value[0].Name);
		}

		public async Task<IEnumerable<NotifierData>> FindAllNotificationsByAggregatorAsync(string aggregator, string hubGuid)
		{
			var query = new QueryRequest(@"SELECT n.*
											FROM DataBucket001 d
											UNNEST d.notifications AS n
											WHERE d.docType='Notifier'
											AND d.aggregator = $aggregator AND d.hubguid=$hubGuid;")
				.AddNamedParameter("$aggregator", aggregator)
				.AddNamedParameter("$hubGuid", hubGuid)
				.Metrics(false);

			return (await _bucket.QueryAsync<NotifierData>(query)).Select(x => x);
		}

		public async Task<NotifierDB> FindByAggregatorAsync(Guid hubguid, string aggregator)
		{
			var query = new QueryRequest(@"SELECT d.*
											FROM DataBucket001 d
											WHERE d.docType='Notifier'
											AND d.aggregator=$aggregator
											AND d.hubguid=$hubGuid")
				.AddNamedParameter("$aggregator", aggregator)
				.AddNamedParameter("$hubGuid", hubguid)
				.Metrics(false);

			return (await _bucket.QueryAsync<NotifierDB>(query)).FirstOrDefault();
		}

		public async Task<NotifierDB> UpsertAsync(NotifierDB notifier)
		{
			var result = await _bucket.UpsertAsync<NotifierDB>(notifier.guid.ToString(), notifier);

			return result.Success ? notifier : null;
		}
	}
}