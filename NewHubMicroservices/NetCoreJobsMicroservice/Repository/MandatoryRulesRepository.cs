using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository
{
    public class MandatoryRulesRepository : IMandatoryRulesRepository
    {
        private readonly IBucket _bucket;
        public MandatoryRulesRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket999");
        }

        public List<MandatoryRules> GetAll()
        {
            var query = new QueryRequest(@"SELECT g.* FROM DataBucket999 g where g.docType = 'MandatoryRules'");
            query.ScanConsistency(ScanConsistency.RequestPlus);
            var result = _bucket.Query<MandatoryRules>(query);
            return result.Success && result.Rows.Count > 0 ? result.Rows : null;
        }

        public MandatoryRules Get(Guid guid)
        {
            var query = new QueryRequest(@"SELECT g.* FROM DataBucket999 g where g.docType = 'MandatoryRules' and g.guid = $guid;")
                .AddNamedParameter("$guid", guid);
            query.ScanConsistency(ScanConsistency.RequestPlus);
            var result = _bucket.Query<MandatoryRules>(query);
            return result.Success && result.Rows.Count > 0 ? result.Rows.SingleOrDefault() : null;
        }

        public List<MandatoryRules> GetByProvider(Guid providerguid)
        {
            var query = new QueryRequest(@"SELECT g.* FROM DataBucket999 g where g.docType = 'MandatoryRules' and g.providerguid = $provguid;")
                .AddNamedParameter("$provguid", providerguid);
            query.ScanConsistency(ScanConsistency.RequestPlus);
            var result = _bucket.Query<MandatoryRules>(query);
            return result.Success && result.Rows.Count > 0 ? result.Rows : null;
        }

        public List<MandatoryRules> GetByProvider(Guid providerguid, string movType)
        {
            var query = new QueryRequest(@"SELECT g.* FROM DataBucket999 g 
where g.docType = 'MandatoryRules' 
and g.providerguid = $provguid
and g.movimenttype = $movType;")
                .AddNamedParameter("$provguid", providerguid)
                .AddNamedParameter("$movType", movType);
            query.ScanConsistency(ScanConsistency.RequestPlus);
            var result = _bucket.Query<MandatoryRules>(query);
            return result.Success && result.Rows.Count > 0 ? result.Rows : null;
        }

        public List<MandatoryRules> GetByProvider(Guid providerguid, string movType, string kinship)
        {
            var query = new QueryRequest(@"SELECT g.* FROM DataBucket999 g 
where g.docType = 'MandatoryRules' 
and g.providerguid = $provguid
and g.movimenttype = $movType
and ARRAY_CONTAINS(g.kinship, $kinship);")
                .AddNamedParameter("$provguid", providerguid)
                .AddNamedParameter("$movType", movType)
                .AddNamedParameter("$kinship", kinship)
                .ScanConsistency(ScanConsistency.RequestPlus);
            var result = _bucket.Query<MandatoryRules>(query);
            return result.Success && result.Rows.Count > 0 ? result.Rows : null;
        }

        public List<MandatoryRules> GetBySegment(string segment)
        {
            var query = new QueryRequest(@"SELECT g.* FROM DataBucket999 g 
where g.docType = 'MandatoryRules' 
and g.segcode = $segment;")
                .AddNamedParameter("$segment", segment);
            query.ScanConsistency(ScanConsistency.RequestPlus);
            var result = _bucket.Query<MandatoryRules>(query);
            return result.Success && result.Rows.Count > 0 ? result.Rows : null;
        }

        public bool Upsert(MandatoryRules rule)
        {
            if (rule.Guid == Guid.Empty)
                rule.Guid = Guid.NewGuid();
             
            var a = _bucket.Upsert(
                rule.Guid.ToString(), new
                {
                    rule.Guid,
                    docType = "MandatoryRules",
                    rule.Providerguid,
                    rule.Segcode,
                    rule.Description,
                    rule.Answer,
                    rule.Kinship,
                    rule.MovimentType,
                    rule.RuleType,
                    rule.Location,
                    rule.Condition
                });

            return a.Success;
        }
    }
}
