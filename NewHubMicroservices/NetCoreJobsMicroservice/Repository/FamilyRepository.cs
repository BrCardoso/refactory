using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository
{
    public class FamilyRepository : IFamilyRepository
	{
		private readonly IBucket _bucket;

		public FamilyRepository(IBucketProvider bucket, IOptions<List<BucketName>> buckets)
		{
			_bucket = bucket.GetBucket(buckets.Value[0].Name);
		}

		public async Task<FamilyDB> UpdatePersonCardNumberAsync(FamilyDB family)
		{
			var result = await _bucket.UpsertAsync(family.guid.ToString(), family);

			if (result.Success)
				return family;

			return null;
		}
	}
}