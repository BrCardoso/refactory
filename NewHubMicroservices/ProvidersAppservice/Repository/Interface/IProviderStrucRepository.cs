using Commons;
using Commons.Base;
using ProvidersAppservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProvidersAppservice.Repository.Interface
{
    public interface IProviderStrucRepository
    {
        bool Upsert(ProviderStrucCB model);
        bool MutateCredentials(Guid guid, Accesscredentials accesscredentials);
        bool MutateProducts(Guid guid, List<ProviderStrucCB.ProductCB> products);
        bool MutateEmails(Guid guid, List<Emailinfo> emailinfos);
        Task<ProviderCustomerCB> Get(Guid guid);
        Task<List<ProviderCustomerCB>> GetAll(Guid token, string aggregator);
        Task<ProviderCustomerCB> GetByProviderGuid(Guid token, string aggregator, Guid provguid);
    }
}
