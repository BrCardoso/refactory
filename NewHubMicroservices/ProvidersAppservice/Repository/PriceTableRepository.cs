using Commons.Base;
using Couchbase;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using ProvidersAppservice.Models;
using ProvidersAppservice.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProvidersAppservice.Repository
{
    public class PriceTableRepository : IPriceTableRepository
    {
        private readonly IBucket _bucket;
        public PriceTableRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        public async Task<PriceTableCB> Get(Guid guid)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'PriceTable' 
and g.guid = $guid;")
                 .AddNamedParameter("$guid", guid)
                 .Metrics(false);

            var a = await _bucket.QueryAsync<PriceTableCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public async Task<List<PriceTableCB>> Get(Guid hubguid, string aggregator)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'PriceTable' 
and g.hubguid = $hubguid 
and g.aggregator = $aggregator;")
                 .AddNamedParameter("$aggregator", aggregator)
                 .AddNamedParameter("$hubguid", hubguid)
                 .Metrics(false);

            var a = await _bucket.QueryAsync<PriceTableCB>(queryRequest);
            return a.Rows;
        }

        public async Task<List<PriceTableCB>> GetByProvider(Guid hubguid, string aggregator, Guid providerguid)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'PriceTable' 
and g.providerguid = $providerguid 
and g.hubguid = $hubguid 
and g.aggregator = $aggregator;")
                 .AddNamedParameter("$providerguid", providerguid)
                 .AddNamedParameter("$aggregator", aggregator)
                 .AddNamedParameter("$hubguid", hubguid)
                 .Metrics(false);

            var a = await _bucket.QueryAsync<PriceTableCB>(queryRequest);
            return a.Rows;
        }

        public async Task<PriceTableCB> GetByProduct(Guid hubguid, string aggregator, Guid providerguid, string providerproductcode)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'PriceTable' 
and g.providerguid = $providerguid 
and g.providerproductcode = $providerproductcode 
and g.hubguid = $hubguid 
and g.aggregator = $aggregator   ;")
                 .AddNamedParameter("$providerguid", providerguid)
                 .AddNamedParameter("$aggregator", aggregator)
                 .AddNamedParameter("$providerproductcode", providerproductcode)
                 .AddNamedParameter("$hubguid", hubguid)
                 .Metrics(false);

            var a = await _bucket.QueryAsync<PriceTableCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public async Task<PriceTableCB> GetByProductSpot(Guid hubguid, string aggregator, Guid providerguid, string providerproductcode, object spotvalue)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT r.*
                                FROM DataBucket001 g
                                    UNNEST g.prices p
                                    UNNEST p.range r
                                where g.docType = 'PriceTable' and g.hubguid = $hubguid and g.aggregator = $aggregator
                                AND g.providerguid = $providerguid
                                AND g.providerproductcode = $product
                                AND 
                                    ( 
                                       (r.initialrange <= $spotvalue AND r.finalrange >= $spotvalue)
                                       or
                                       (p.type = 'FIXO')
                                    );")
                 .AddNamedParameter("$hubguid", hubguid)
                 .AddNamedParameter("$aggregator", aggregator)
                 .AddNamedParameter("$providerguid", providerguid)
                   .AddNamedParameter("$product", providerproductcode)
                   .AddNamedParameter("$spotvalue", spotvalue)
                 .Metrics(false);

            var a = await _bucket.QueryAsync<PriceTableCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public async Task<PriceTable> GetByName(Guid hubguid, string aggregator, Guid providerguid, string providerproductcode, object tablename)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT p.*
                                FROM DataBucket001 g
                                    UNNEST g.prices p
                                where g.docType = 'PriceTable' and g.hubguid = $hubguid and g.aggregator = $aggregator
                                AND g.providerguid = $providerguid
                                AND g.providerproductcode = $product
                                AND p.name = $tablename;")
                 .AddNamedParameter("$hubguid", hubguid)
                 .AddNamedParameter("$aggregator", aggregator)
                 .AddNamedParameter("$providerguid", providerguid)
                 .AddNamedParameter("$product", providerproductcode)
                 .AddNamedParameter("$tablename", tablename)
                 .Metrics(false);

            var a = await _bucket.QueryAsync<PriceTable>(queryRequest);
            return a.SingleOrDefault();
        }

        public IOperationResult Upsert(PriceTableCB model)
        {
            foreach (var price in model.prices)
            {
                price.creationdate = DateTime.Now;
            }
            model.guid = Guid.NewGuid();
            var a = _bucket.Upsert(
            model.guid.ToString(), new
            {
                model.guid,
                model.hubguid,
                docType = "PriceTable",
                model.providerguid,
                model.providerproductcode,
                model.aggregator,
                model.prices
            });

            return a;
        }

        public bool MutatePrices(Guid guid, List<PriceTable> prices)
        {
            var result = _bucket.MutateIn<dynamic>(guid.ToString()).Upsert("prices", prices).Execute();
            return result.Success;
        }
    }
}
