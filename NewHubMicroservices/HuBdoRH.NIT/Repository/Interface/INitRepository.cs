using Commons.Base;
using Couchbase.N1QL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HuBdoRH.NIT.Repository.Interface
{   
    public interface INitRepository
    {
        Task<IQueryResult<Nit.NitModel>> GetTasksAsync();
        Task<bool> InsertNitTaskListAsync(List<Nit.NitModel> nitTasks);
        Task<bool> InsertNitTaskAsync(Nit.NitModel nitTask);
        Task<bool> UpdateNitTaskAsync(Nit.NitModel nitTasks);
        Task<Nit.NitModel> FindByNitGuidAsync(Guid nitTaskGuid);

        /*Task<List<ChargeOrder>> GetListAsync(Guid hubguid, string aggregator, string competence);
        Task<List<ChargeOrder>> GetListAsync(Guid hubguid, string aggregator, string competence, string subsegcode);
        Task<ChargeOrder> GetAsync(Guid hubguid, string aggregator, Guid docid);
        ChargeOrder Upsert(ChargeOrder oCharges);
        Task<ChargeOrder> DuplicateAsync(Guid hubguid, Guid providerguid, Guid ruleguid, string aggregator, string competencia, string subsegcode);*/
    }
}
