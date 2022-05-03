using chargeAppServices.Models;
using System.Threading.Tasks;

namespace ChargeAppServices.Repository.Interface
{
    public interface IHolydaysRepository
    {
        Task<Holidays> Get();
    }
}
