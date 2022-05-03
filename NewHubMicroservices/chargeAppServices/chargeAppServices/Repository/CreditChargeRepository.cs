using chargeAppServices.Models;
using ChargeAppServices.Repository.Interface;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ChargeAppServices.Repository
{
    public class CreditChargeRepository : ICreditChargeRepository
    {
        private readonly IBucket _bucket;


        public CreditChargeRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        public async Task<ChargeOrder> DuplicateAsync(Guid hubguid, Guid providerguid, Guid ruleguid, string aggregator, string competencia, string subsegcode)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'creditcharge' 
AND g.hubguid = $hubguid
AND g.providercustomerguid = $providercustomerguid
AND g.rulesconfigurationguid = $rulesconfigurationguid
AND g.aggregator  = $aggregator 
AND g.competence  = $competence 
AND g.subsegcode  = $subsegcode ; ")
                  .AddNamedParameter("$hubguid", hubguid)
                  .AddNamedParameter("$providercustomerguid", providerguid)
                  .AddNamedParameter("$rulesconfigurationguid", ruleguid)
                  .AddNamedParameter("$aggregator", aggregator)
                  .AddNamedParameter("$competence", competencia)
                  .AddNamedParameter("$subsegcode", subsegcode)
                  .Metrics(false);

            var a = await _bucket.QueryAsync<ChargeOrder>(queryRequest);
            return a.Success ? a.SingleOrDefault() : null;
        }

        public async Task<ChargeOrder> GetAsync(Guid hubguid, string aggregator, Guid docid)
        {
            var queryRequest = new QueryRequest()
                          .Statement(string.Format(@"SELECT g.* FROM DataBucket001 g
WHERE g.docType = 'creditcharge' 
AND g.hubguid = $hubguid  
AND g.aggregator = $aggregator  
AND g.guid = $guid;"))
                          .AddNamedParameter("$hubguid", hubguid)
                          .AddNamedParameter("$aggregator", aggregator)
                          .AddNamedParameter("$guid", docid)
                          .Metrics(false);
            var result = await _bucket.QueryAsync<ChargeOrder>(queryRequest);
            return result.Success ? result.SingleOrDefault() : null;
        }

        public async Task<List<ChargeOrder>> GetListAsync(Guid hubguid, string aggregator, string competence)
        {
            var queryRequest = new QueryRequest()
                         .Statement(string.Format(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'creditcharge' 
AND g.hubguid = $hubguid 
AND g.aggregator = $aggregator 
AND g.competence = $competence;"))
                         .AddNamedParameter("$hubguid", hubguid)
                         .AddNamedParameter("$aggregator", aggregator)
                         .AddNamedParameter("$competence", competence)
                         .Metrics(false);
            var result = await _bucket.QueryAsync<ChargeOrder>(queryRequest);
            return result.Success ? result.Rows : null;
        }

        public async Task<List<ChargeOrder>> GetListAsync(Guid hubguid, string aggregator, string competence, string subsegcode)
        {
            var queryRequest = new QueryRequest()
                         .Statement(string.Format(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'creditcharge' 
AND g.hubguid = $hubguid 
AND g.aggregator = $aggregator 
AND g.subsegcode = $subsegcode 
AND g.competence = $competence;"))
                         .AddNamedParameter("$hubguid", hubguid)
                         .AddNamedParameter("$aggregator", aggregator)
                         .AddNamedParameter("$subsegcode", subsegcode)
                         .AddNamedParameter("$competence", competence)
                         .Metrics(false);
            var result = await _bucket.QueryAsync<ChargeOrder>(queryRequest);
            return result.Success ? result.Rows : null;
        }

        public ChargeOrder Upsert(ChargeOrder modelCharge)
        {
            if (modelCharge.Guid == Guid.Empty)
                modelCharge.Guid = Guid.NewGuid();

            var a = _bucket.Upsert(
                    modelCharge.Guid.ToString(), new
                    {
                        modelCharge.Guid,
                        docType = "creditcharge",
                        modelCharge.Hubguid,
                        modelCharge.Providercustomerguid,
                        modelCharge.Rulesconfigurationguid,
                        modelCharge.Aggregator,
                        modelCharge.Competence,
                        modelCharge.Subsegcode,
                        modelCharge.Charges
                    }
                );

            return a.Success ? modelCharge : null;
        }
    }
}
