using Couchbase;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.Linq;
using Couchbase.Linq.Extensions;

using IdentityService.Models;
using IdentityService.Repository.Interfaces;

using System.Linq;
using System.Threading.Tasks;

namespace IdentityService.Repository
{
	public class IdentityTokensRepository : IIdentityTokensRepository
	{
		private readonly IBucket _bucket;
		private readonly IBucketContext _context;

		public IdentityTokensRepository(IBucketProvider bucketProvider)
		{
			IBucket bucket = bucketProvider.GetBucket("DataBucket001");
			_bucket = bucket;
			_context = new BucketContext(bucket);
		}

		public async Task<IdentityTokensModel> FindAsync() =>
			(await _context.Query<IdentityTokensModel>().Where(x => x.DocType == "IdentityTokens").ExecuteAsync()).SingleOrDefault();

		public async Task<IdentityTokensModel> UpSertAsync(IdentityTokensModel identityTokens)
		{
			IOperationResult<IdentityTokensModel> result = await _bucket.UpsertAsync(identityTokens.Guid.ToString(), identityTokens);
			return result.Success ? identityTokens : null;
		}
	}
}