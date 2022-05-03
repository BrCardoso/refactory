using Commons.Base;
using Couchbase;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using HuBdoRH.NIT.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuBdoRH.NIT.Repository
{
    public class NitRepository : INitRepository
    {
        private readonly IBucket _bucket;
        private readonly string NITBUCKET = "DataBucket001";

        public NitRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket(NITBUCKET);
        }

        public async Task<IQueryResult<Nit.NitModel>> GetTasksAsync()
        {
            var query = new QueryRequest("SELECT t.* FROM " + NITBUCKET + " AS t WHERE t.docType = 'NitTask' ORDER BY g.createdDate DESC").Metrics(false);

            return await _bucket.QueryAsync<Nit.NitModel>(query);
        }

        public async Task<bool> InsertNitTaskListAsync(List<Nit.NitModel> taskList)
        {
            try
            {
                //Valida se todos os itens da lista estão com guid vazio, i.e., permite inclusao
                foreach (Nit.NitModel task in taskList)
                {
                    task.guid = Guid.NewGuid();
                    task.docType = "NitTask";

                    var retDb = await _bucket.UpsertAsync(task.guid.ToString(), task);

                    if (!retDb.Success)
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> InsertNitTaskAsync(Nit.NitModel task)
        {
            try
            {

                task.guid = Guid.NewGuid();
                task.docType = "NitTask";

                var retDb = await _bucket.UpsertAsync(task.guid.ToString(), task);

                return (retDb.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> UpdateNitTaskAsync(Nit.NitModel task)
        {
            try
            {
                var retDb = await _bucket.UpsertAsync(task.guid.ToString(), task);

                return (retDb.Success);
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<Nit.NitModel> FindByNitGuidAsync(Guid nitTaskGuid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.guid= $guid;")
                .AddNamedParameter("$guid", nitTaskGuid)
                .AddNamedParameter("$docType", "NitTask")
                .Metrics(false);

            var result = await _bucket.QueryAsync<Nit.NitModel>(query);

            return result.FirstOrDefault();
        }
    }
}
