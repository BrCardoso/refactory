using chargeAppServices.Models;
using ChargeAppServices.Repository.Interface;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using System.Linq;
using System.Threading.Tasks;

namespace ChargeAppServices.Repository
{
    public class HolidaysRepository : IHolydaysRepository
    {
        private readonly IBucket _bucket;
        public HolidaysRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket999");
        }
        public async Task<Holidays> Get()
        {
            var queryRequest = new QueryRequest()
                .Statement(@"SELECT g.* FROM DataBucket999 g where g.docType = 'Holidays';")
                .Metrics(false);
            var result = await _bucket.QueryAsync<Holidays>(queryRequest);
            return result.Success ? result.SingleOrDefault() : null;
        }
    }
}
