using Couchbase;
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

namespace NetCoreJobsMicroservice.Repository
{
	public class ConferenceRepository : IConferenceRepository
	{
		private readonly IBucket _bucket;

		public ConferenceRepository(IBucketProvider bucket, IOptions<List<BucketName>> buckets)
		{
			_bucket = bucket.GetBucket(buckets.Value[0].Name);
		}

		public async Task<IEnumerable<ConferenceDB.Conference<ConferenceDB.Error>>> FindAllInvoicesAsync(string aggregator, Guid hubguid)
		{
			var query = new QueryRequest()
				.Statement(@"SELECT aggregator,contracts, status,copartvalue,dispatch,docType,file,guid,hubguid,incdate,invvalue,providerguid,ratevalue,
							receipt, receiptnumber, refdate FROM DataBucket001
							WHERE docType = 'InvoiceValidation' AND hubguid = $hubguid AND aggregator = $aggregator; ")
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$aggregator", aggregator)
				.Metrics(false);

			var result = await _bucket.QueryAsync<ConferenceDB.Conference<ConferenceDB.Error>>(query);
			return result.Select(x => x);
		}

		public async Task<ConferenceDB.Conference<ConferenceDB.Error>> FindDocByGuidAndDocTypeAsync(Guid guid, Guid hubguid, string aggregator)
		{
			var query = new QueryRequest()
				.Statement("select d.* from DataBucket001 d WHERE d.docType='InvoiceValidation' AND d.guid=$guid and hubguid=$hubguid  AND aggregator=$aggregator")
				.AddNamedParameter("$guid", guid)
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$aggregator", aggregator)
				.Metrics(false);

			var result = await _bucket.QueryAsync<ConferenceDB.Conference<ConferenceDB.Error>>(query);
			return result.SingleOrDefault(x => x.guid == guid);
		}

		public async Task<ConferenceDB.Conference<ConferenceDB.Error>> FindInvoiceDetailsAsync(Guid guid, Guid hubguid, string aggregator)
		{
			var query = new QueryRequest()
				.Statement(@"SELECT d.* FROM DataBucket001 d Where docType='InvoiceValidation' AND guid=$guid and hubguid=$hubguid  AND aggregator=$aggregator")
				.AddNamedParameter("$guid", guid)
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$aggregator", aggregator)
				.Metrics(false);

			var result = await _bucket.QueryAsync<ConferenceDB.Conference<ConferenceDB.Error>>(query);

			if (!(result.SingleOrDefault(x => x.guid == guid) is ConferenceDB.Conference<ConferenceDB.Error> conference))
				return null;

			return conference;
		}

		public async Task<ConferenceExtractDB.ConferenceExtract> FindExtracsByProviderAsync(Guid hubguid, string aggregator, Guid providerguid)
		{
			var query = new QueryRequest()
				.Statement(@"SELECT d.* FROM `DataBucket001` d WHERE docType='LeagerAccount' AND hubguid=$hubguid AND providerguid=$providerguid AND aggregator=$aggregator")
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$providerguid", providerguid)
				.AddNamedParameter("$aggregator", aggregator)
				.Metrics(false);

			var result = await _bucket.QueryAsync<ConferenceExtractDB.ConferenceExtract>(query);
			return result.SingleOrDefault(x => x.providerguid == providerguid);
		}

		public async Task<ConferenceDB.Conference<ConferenceDB.Error>> FindByRefDateAsync(Guid hubguid, string aggregator, Guid providerguid, string refdate, IEnumerable<string> contracts)
		{
			var query = new QueryRequest()
				.Statement("SELECT d.* FROM `DataBucket001` d WHERE d.docType='InvoiceValidation' AND d.hubguid=$hubguid AND d.aggregator=$aggregator AND d.providerguid=$providerguid AND d.refdate=$refdate AND (ARRAY_LENGTH(ARRAY_INTERSECT(d.contracts, $contracts)) == $contractsCount);")
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$aggregator", aggregator)
				.AddNamedParameter("$providerguid", providerguid)
				.AddNamedParameter("$refdate", refdate)
				.AddNamedParameter("$contracts", contracts)
				.AddNamedParameter("$contractsCount", contracts.Count())
				.Metrics(false);

			var result = await _bucket.QueryAsync<ConferenceDB.Conference<ConferenceDB.Error>>(query);
			return result.SingleOrDefault(x => x.providerguid == providerguid);
		}

		public async Task<ConferenceExtractDB.ConferenceExtract> UpsertExtractAsync(ConferenceExtractDB.ConferenceExtract conferenceExtract)
		{
			var result = await _bucket.UpsertAsync(conferenceExtract.guid.ToString(), conferenceExtract);

			if (result.Success)
				return conferenceExtract;

			return null;
		}

		public async Task<ConferenceDB.Conference<ConferenceDB.ErrorRequired>> UpsertAsync(ConferenceDB.Conference<ConferenceDB.ErrorRequired> dataInvoice)
		{
			IOperationResult<ConferenceDB.Conference<ConferenceDB.ErrorRequired>> result = await _bucket.UpsertAsync(dataInvoice.guid.ToString(), dataInvoice);

			if (result.Success)
				return dataInvoice;

			return null;
		}

		public async Task<ConferenceDB.Conference<ConferenceDB.Error>> UpsertAsync(ConferenceDB.Conference<ConferenceDB.Error> dataInvoice)
		{
			IOperationResult<ConferenceDB.Conference<ConferenceDB.Error>> result = await _bucket.UpsertAsync(dataInvoice.guid.ToString(), dataInvoice);

			if (result.Success)
				return dataInvoice;

			return null;
		}

		public async Task<bool> DeleteByGuidAsync(Guid guid, Guid hubguid, string aggregator)
		{
			var query = new QueryRequest()
				.Statement("DELETE FROM DataBucket001 WHERE docType='InvoiceValidation' and hubguid = $hubguid and aggregator = $aggregator AND guid=$guid RETURNING meta().id")
				.AddNamedParameter("$guid", guid)
				.AddNamedParameter("$hubguid", hubguid)
				.AddNamedParameter("$aggregator", aggregator)
				.Metrics(false);

			var result = await _bucket.QueryAsync<MetaResult>(query);

			return result.Any(x => x.id == guid);
		}
	}
}