using chargeAppServices.Models;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest.Interface
{
    public interface ILoginRequest
    {
        Task<Login.ApplicationResponseModel> GetAsync();
    }
}
