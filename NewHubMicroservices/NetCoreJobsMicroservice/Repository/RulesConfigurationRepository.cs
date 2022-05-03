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
    public class RulesConfigurationRepository : IRulesConfigurationRepository
    {
        private readonly IBucket _bucket;
        public RulesConfigurationRepository(IBucketProvider provider)
        {
            _bucket = provider.GetBucket("DataBucket001");
        }

        public string findRC(Guid hubguid, Guid? docid, Guid providerguid, string contractnumber)
        {
            try
            {
                var query = new QueryRequest(
                    @"SELECT g.* FROM DataBucket001 g 
where g.docType = 'RulesConfiguration' 
and g.hubguid = $hubguid
and g.guid = $guid
or
g.docType = 'RulesConfiguration' 
and g.hubguid = $hubguid
and g.providerguid = $providerguid
and g.contractnumber = $contract
")
                    .AddNamedParameter("$hubguid", hubguid)
                    .AddNamedParameter("$guid", docid)
                    .AddNamedParameter("$providerguid", providerguid)
                    .AddNamedParameter("$contract", contractnumber);

                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<TaskPanel>(query);
                if (result.Success && result.Rows.Count > 0)
                {
                    return result.Rows[0].guid.ToString();
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return ex.ToString();
            }
        }

        public RulesConfigurationModel getRC(Guid hubguid, Guid? docid, Guid providerguid, string contractnumber)
        {
            try
            {
                var query = new QueryRequest(
                    @"SELECT g.* FROM DataBucket001 g 
where g.docType = 'RulesConfiguration' 
and g.hubguid = $hubguid
and g.guid = $guid
or
g.docType = 'RulesConfiguration' 
and g.hubguid = $hubguid
and g.providerguid = $providerguid
and TRIM(g.contractnumber) = $contract
")
                    .AddNamedParameter("$hubguid", hubguid)
                    .AddNamedParameter("$guid", docid)
                    .AddNamedParameter("$providerguid", providerguid)
                    .AddNamedParameter("$contract", contractnumber.Trim());

                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<RulesConfigurationModel>(query);
                if (result.Success && result.Rows.Count > 0)
                {
                    return result.Rows[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        public async Task<List<RulesConfigurationModel>> getRCByContractsAsync(Guid token, Guid provider, string contract)
        {
            var queryRequest = new QueryRequest()
                    .Statement(@"SELECT g.*
                                FROM DataBucket001 g
                                WHERE g.docType = 'RulesConfiguration'
                                AND g.hubguid = $hubguid
                                AND g.providerguid = $providerguid
                                AND TRIM(g.contractnumber) = $contract
                                order by g.description
                            ")
                    .AddNamedParameter("$hubguid", token.ToString())
                    .AddNamedParameter("$providerguid", provider.ToString())
                    .AddNamedParameter("$contract", contract.Trim())
                    .Metrics(false);

            var a = await _bucket.QueryAsync<RulesConfigurationModel>(queryRequest);
            return a.Success && a.Rows.Count > 0 ? a.Rows : null;
        }
    }
}
