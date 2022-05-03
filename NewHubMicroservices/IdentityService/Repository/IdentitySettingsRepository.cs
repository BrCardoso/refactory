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
	public class IdentitySettingsRepository : IIdentitySettingsRepository
	{
		private readonly IBucket _bucket;
		private readonly IBucketContext _context;

		public IdentitySettingsRepository(IBucketProvider bucketProvider)
		{
			IBucket bucket = bucketProvider.GetBucket("DataBucket001");
			_bucket = bucket;
			_context = new BucketContext(bucket);
		}

		public async Task<IdentitySettingsModel> FindAsync()
		{
			return (await _context.Query<IdentitySettingsModel>().Where(x => x.DocType == "IdentitySettings").ExecuteAsync())?.SingleOrDefault();
		}
	}
}