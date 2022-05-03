using Commons.Base;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using InputAppService.Models;
using InputAppService.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InputAppService.Repository
{
    public class EntityRepository : IEntityRepository
    {
        private readonly IBucket _bucket;
        public EntityRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        public async Task<EntityModel> GetAsync(Guid hubguid, string aggregator)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Entity' 
and  g.hubguid = $hubguid
    and g.aggregator = $aggregator;")
                 .AddNamedParameter("$hubguid", hubguid)
                 .AddNamedParameter("$aggregator", aggregator)
                 .Metrics(false);
            var a = await _bucket.QueryAsync<EntityModel>(queryRequest);

            if (a.Success && a.Rows.Count > 0)
                return a.SingleOrDefault();
            return null;
        }
        public async Task<EntityModel> GetAsync(Guid hubguid, string aggregator, Guid provguid)
        {
            var queryRequest = new QueryRequest()
                 .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Entity' 
and  g.hubguid = $hubguid
and g.aggregator = $aggregator
and g.providerguid = $provguid;")
                 .AddNamedParameter("$hubguid", hubguid)
                 .AddNamedParameter("$aggregator", aggregator)
                   .AddNamedParameter("$provguid", provguid)
                 .Metrics(false);
            var a = await _bucket.QueryAsync<EntityModel>(queryRequest);

            if (a.Success && a.Rows.Count > 0)
                return a.SingleOrDefault();
            return null;
        }

        public bool Mutate(Guid guid, List<Entity> entities)
        {
            var a = _bucket.MutateIn<CopartModel>(guid.ToString())
                                .Upsert("entities", entities, true)
                                .Execute();
            return a.Success;
        }

        public bool Upsert(EntityModel model)
        {
            model.guid = Guid.NewGuid();
            //insere novo doc no DB
            var a = _bucket.Upsert(
                model.guid.ToString(), new
                {
                    model.guid,
                    docType = "Entity",
                    hubguid = model.hubguid,
                    model.aggregator,
                    model.entities
                });
            return a.Success;
        }
    }
}
