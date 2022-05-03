using chargeAppServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargeAppServices.Business.Interface
{
    public interface ICreditChargeBusiness
    {
        Task<ChargeOrder> CalculaFoodAsync(List<rulesConfigurationModel> result, string authorization);
        List<ChargeOrder> Upsert(List<ChargeOrder> oCharges);
    }
}
