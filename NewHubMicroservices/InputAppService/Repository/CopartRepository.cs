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
    public class CopartRepository : ICopartRepository
    {
        private readonly IBucket _bucket;
        public CopartRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        public async Task<List<CopartModel>> Get(Guid token, string aggregator)
        {
            var queryRequest = new QueryRequest()
                               .Statement(@"SELECT g.* FROM DataBucket001 g where g.docType = 'Coparticipation' and g.hubguid = $hubguid and g.aggregator = $aggregator")
                               .AddNamedParameter("$hubguid", token)
                               .AddNamedParameter("$aggregator", aggregator)
                               .Metrics(false);

            var result = await _bucket.QueryAsync<CopartModel>(queryRequest);
            if (result.Success)
                if (result.Rows.Count > 0)
                    return result.Rows;

            return null;
        }

        public async Task<CopartModel> Get(Guid token, string aggregator, Guid provider)
        {
            var queryRequest = new QueryRequest()
                   .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Coparticipation' 
and g.hubguid = $hubguid 
and g.providerguid = $provguid 
and g.aggregator = $aggregator;")
                   .AddNamedParameter("$hubguid", token)
                   .AddNamedParameter("$provguid", provider)
                   .AddNamedParameter("$aggregator", aggregator)
                   .Metrics(false);

            var result = await _bucket.QueryAsync<CopartModel>(queryRequest);
            if (result.Success && result.Rows.Count > 0)
                return result.SingleOrDefault();
            return null;
        }

        public bool Mutate(Guid guid, List<Coparticipation> Coparticipations)
        {
            var a = _bucket.MutateIn<CopartModel>(guid.ToString())
                                     .Upsert("Coparticipations", Coparticipations, true)
                                     .Execute();
            return a.Success;
        }

        public bool Upsert(CopartModel model)
        {
            model.guid = Guid.NewGuid();
            //insere novo doc no DB
            var a = _bucket.Upsert(
                model.guid.ToString(), new
                {
                    model.guid,
                    docType = "Coparticipation",
                    hubguid = model.hubguid,
                    model.aggregator,
                    model.Coparticipations
                });

            return a.Success;
        }
    }
}
