using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;

using LoginAppService.Model;
using LoginAppService.Repository.Interfaces;

using System.Linq;
using System.Threading.Tasks;

namespace LoginAppService.Repository
{
	public class ConfigPasswordRepository : IConfigPasswordRepository
	{
		private readonly IBucket _bucket;

		public ConfigPasswordRepository(IBucketProvider bucket)
		{
			_bucket = bucket.GetBucket("DataBucket001");
		}

		public async Task<ConfigPassword> FindAsync()
		{
			var query = new QueryRequest()
				.Statement(@"SELECT d.*
							FROM DataBucket001 d
							WHERE d.docType = 'ConfigPassword'")
				.Metrics(false);

			var result = await _bucket.QueryAsync<ConfigPassword>(query);

			return result.SingleOrDefault();
		}

		public async Task<bool> UpSertAsync(ConfigPassword configPassword)
		{
			var result = await _bucket.UpsertAsync(configPassword.guid.ToString(), configPassword);

			return result.Success;
		}
	}
}