using chargeAppServices.Models;
using Commons.Base;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargeAppServices.Repository.Interface
{
    public interface ICreditChargeRepository
    {
        Task<List<ChargeOrder>> GetListAsync(Guid hubguid, string aggregator, string competence);
        Task<List<ChargeOrder>> GetListAsync(Guid hubguid, string aggregator, string competence, string subsegcode);
        Task<ChargeOrder> GetAsync(Guid hubguid, string aggregator, Guid docid);
        ChargeOrder Upsert(ChargeOrder oCharges);
        Task<ChargeOrder> DuplicateAsync(Guid hubguid, Guid providerguid, Guid ruleguid, string aggregator, string competencia, string subsegcode);

    }
}
