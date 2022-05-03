using Commons;
using ProvidersAppservice.Models;
using System.Threading.Tasks;

namespace ProvidersAppservice.Business.Interface
{
    public interface IProviderStrucBusiness
    {
        Task<ProviderCustomerCB> GetProductNames(ProviderCustomerCB model);
        Task<MethodFeedback> UpsertAsync(ProviderStrucCB model);
    }
}
