using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using Commons;
using Couchbase.N1QL;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service.Interfaces
{
    public interface IBeneficiaryService
    {
        Task<MethodFeedback> UpsertBenefit(TaskResultModel model,string authorization);

        Task<MethodFeedback> SolCard(CardReissue cr,string authorization);
        Task<FamilyOut> UpsertFamilyAsync(FamilyIn inputFamily, Guid hubguid, string aggregator, string authorization);
        Task<List<ValidateByContractModel>> ValidateFamiliesByContract(RulesConfigurationModel RC, string authorization);
        Task<List<FamilyFull>> GetFamiliesFull(IQueryResult<FamilyFull> result);
    }
}
