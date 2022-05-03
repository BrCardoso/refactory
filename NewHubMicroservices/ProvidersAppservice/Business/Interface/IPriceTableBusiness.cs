using Commons;
using ProvidersAppservice.Models;
using System.Threading.Tasks;

namespace ProvidersAppservice.Business.Interface
{
    public interface IPriceTableBusiness
    {
        Task<MethodFeedback> Upsert(PriceTableCB model);
    }
}
