using Commons.Base;
using Couchbase;
using ProvidersAppservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProvidersAppservice.Repository.Interface
{
    public interface IPriceTableRepository
    {
        Task<PriceTableCB> Get(Guid guid);
        Task<List<PriceTableCB>> Get(Guid hubguid, string aggregator);
        Task<List<PriceTableCB>> GetByProvider(Guid hubguid, string aggregator, Guid providerguid);
        Task<PriceTableCB> GetByProduct(Guid hubguid, string aggregator, Guid providerguid, string providerproductcode);
        Task<PriceTableCB> GetByProductSpot(Guid hubguid, string aggregator, Guid providerguid, string providerproductcode, object spotvalue);
        Task<PriceTable> GetByName(Guid hubguid, string aggregator, Guid providerguid, string providerproductcode, object tablename);
        IOperationResult Upsert(PriceTableCB model);
        bool MutatePrices(Guid guid, List<PriceTable> prices);
    }
}
