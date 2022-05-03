using chargeAppServices.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest.Interface
{
    public interface IRulesConfigurationRequest
    {
        Task<List<rulesConfigurationModel>> GetAsync(string segcode, string authorization);
    }
}
