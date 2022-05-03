using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using NotifierAppService.Models;
using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository
{
    public class TaskPanelRepository : ITaskPanelRepository
    {
        private readonly IConfiguration _configuration;
        private readonly IBucket _bucket;
        public TaskPanelRepository(IConfiguration configuration, IBucketProvider provider)
        {
            _configuration = configuration;
            _bucket = provider.GetBucket("DataBucket001");
        }

        public async Task<bool> AddTaskPanelAsync(TaskPanel taskPanel)
        {
            if (taskPanel != null)
            {

                taskPanel.guid = Guid.NewGuid();
                taskPanel.incdate = DateTime.Now;
                var a = _bucket.Upsert(
                   taskPanel.guid.ToString(), new
                   {
                       taskPanel.guid,
                       docType = "Task",
                       taskPanel.incdate,
                       taskPanel.status,
                       taskPanel.origin,
                       taskPanel.personguid,
                       taskPanel.hubguid,
                       taskPanel.aggregator,
                       taskPanel.providerguid,
                       taskPanel.mandatoryrulesguids,
                       taskPanel.subject,
                       taskPanel.obs,
                       taskPanel.response,
                       taskPanel.conclusiondate,
                       taskPanel.divergence,
                       taskPanel.movtype,
                       taskPanel.beneficiarydetails,
                       taskPanel.benefitinfos
                   });
                return a.Success;
            }
            return false;
        }

        public TaskPanel findTask(Guid hubguid, Guid docid, Guid personguid, string movType)
        {
            try
            {
                var query = new QueryRequest(
                    @"SELECT g.* FROM DataBucket001 g 
where g.docType = 'Task' 
and g.hubguid = $hubguid
and g.guid = $guid
or
g.docType = 'Task' 
and g.hubguid = $hubguid
and g.personguid = $personguid
and g.movtype = $movType;
")


                    .AddNamedParameter("$hubguid", hubguid)
                    .AddNamedParameter("$guid", docid)
                    .AddNamedParameter("$personguid", personguid)
                    .AddNamedParameter("$movType", movType);

                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<TaskPanel>(query);
                if (result.Success && result.Rows.Count > 0)
                {
                    return result.Rows[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
