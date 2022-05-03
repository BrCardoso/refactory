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
	public class UserRepository : IUserRepository
	{
		private readonly IBucket _bucket;
		private readonly IBucketContext _context;

		public UserRepository(IBucketProvider bucketProvider)
		{
			IBucket bucket = bucketProvider.GetBucket("DataBucket001");
			_bucket = bucket;
			_context = new BucketContext(bucket);
		}

		public async Task<IdentityModel> FindAsync()
		{
			return (await _context.Query<IdentityModel>().Where(x => x.DocType == "Identity").ExecuteAsync())?.SingleOrDefault();
		}

		public async Task<IdentityModel> UpSertAsync(IdentityModel identity)
		{
			IOperationResult<IdentityModel> result = await _bucket.UpsertAsync(identity.Guid.ToString(), identity);
			return result.Success ? identity : null;
		}
	}
}