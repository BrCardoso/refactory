using ProvidersAppservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ProvidersAppservice.Repository.Interface
{
    public interface IProviderRepository
    {
        Task<ProviderCB> Upsert(ProviderCB provider);
        Task<ProviderCB> Get(Guid token);
        Task<List<ProviderCB>> Get();
        Task<ProviderCB> FindByCNPJ(string cnpj);
        Task<ProviderCB> FindByRegistration(string registration);
        Task<ProviderCB> FindByName(string name);
    }
}
