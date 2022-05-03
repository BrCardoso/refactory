using Couchbase.Core;
using Couchbase.N1QL;
using InputAppService.Models;
using System.Threading.Tasks;

namespace InputAppService
{
    public class operations {
        
        internal static async Task<IQueryResult<InsuranceClaimModel>> findInsuranceClaimAsync(InsuranceClaimModel insclaim, IBucket _bucket)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'InsuranceClaim' 
and ( 
    g.hubguid = $hubguid
    and g.aggregator = $aggregator
);")
                  .AddNamedParameter("$hubguid", insclaim.hubguid)
                  .AddNamedParameter("$aggregator", insclaim.aggregator)
                  .Metrics(false);

            return await _bucket.QueryAsync<InsuranceClaimModel>(queryRequest);
        }
        
    }
}
