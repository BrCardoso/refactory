using Commons;
using Commons.Base;
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
    public class ProviderStrucRepository : IProviderStrucRepository
    {
        private readonly IBucket _bucket;
        public ProviderStrucRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }
        public async Task<ProviderCustomerCB> Get(Guid guid)
        {
            var queryRequest = new QueryRequest()
                     .Statement(@"SELECT p.*, g.*
FROM DataBucket001 AS g  INNER JOIN DataBucket001 p ON KEYS g.providerguid 
WHERE g.docType = 'ProviderStruc' and g.guid = $guid;")
                     .AddNamedParameter("$guid", guid)
                     .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCustomerCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public async Task<List<ProviderCustomerCB>> GetAll(Guid token, string aggregator)
        {
            var queryRequest = new QueryRequest()
                     .Statement(@"SELECT p.*, g.*
FROM DataBucket001 AS g  INNER JOIN DataBucket001 p ON KEYS g.providerguid 
where g.docType = 'ProviderStruc' and g.hubguid = $hubguid and g.aggregator = $aggregator;")
                   .AddNamedParameter("$hubguid", token)
                   .AddNamedParameter("$aggregator", aggregator)
                     .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCustomerCB>(queryRequest);
            return a.Rows;
        }

        public async Task<ProviderCustomerCB> GetByProviderGuid(Guid token, string aggregator, Guid provguid)
        {
            var queryRequest = new QueryRequest()
                     .Statement(@"SELECT p.*, g.*
FROM DataBucket001 AS g  INNER JOIN DataBucket001 p ON KEYS g.providerguid 
WHERE g.docType = 'ProviderStruc'  
and g.hubguid = $hubguid and g.providerguid = $providerguid and g.aggregator = $aggregator;")
                     .AddNamedParameter("$hubguid", token)
                     .AddNamedParameter("$providerguid", provguid)
                     .AddNamedParameter("$aggregator", aggregator)
                     .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCustomerCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public bool Upsert(ProviderStrucCB model)
        {
            model.Guid = Guid.NewGuid();
            var a = _bucket.Upsert(
            model.Guid.ToString(), new
            {
                model.Guid,
                model.Hubguid,
                model.Providerguid,
                docType = "ProviderStruc",
                model.Aggregator,
                model.Code,
                model.Emailinfos,
                model.Products,
                model.accesscredentials
            });

            return a.Success;
        }
        public bool MutateCredentials(Guid guid, Accesscredentials accesscredentials)
        {
            var result = _bucket.MutateIn<dynamic>(guid.ToString()).Upsert("accesscredentials", accesscredentials).Execute();
            return result.Success;
        }
        public bool MutateEmails(Guid guid, List<Emailinfo> emailinfos)
        {
            var result = _bucket.MutateIn<dynamic>(guid.ToString()).Upsert("emailinfos", emailinfos).Execute();
            return result.Success;
        }
        public bool MutateProducts(Guid guid, List<ProviderStrucCB.ProductCB> products)
        {
            var result = _bucket.MutateIn<dynamic>(guid.ToString()).Upsert("products", products).Execute();
            return result.Success;
        }
    }
}
