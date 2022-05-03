using BeneficiaryAppService.Models;
using Commons.Base;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface IBenefitService
    {
        Task<FamilyFull> blockBenefitAsync(BenefitTransactionModel.BenefitDocument model, string authorization);
    }
}
