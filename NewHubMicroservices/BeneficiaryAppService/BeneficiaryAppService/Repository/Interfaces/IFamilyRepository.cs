using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeneficiaryAppService.Models;
using Couchbase;
using Couchbase.N1QL;

namespace BeneficiaryAppService.Repository.Interfaces
{
    public interface IFamilyRepository
    {
        Task<IOperationResult<FamilyDb>> AddAsync(FamilyDb newDocument);
        Task<IOperationResult<FamilyDb>> UpSert(FamilyDb document);
        Task<IOperationResult<FamilyDb>> UpSertBeneficiary(TaskResultModel model, PersonDB findPerson, PersonDB findEmployee, string authorization);
        bool AppendNewBeneficiary(Guid docid, BeneficiaryDb beneficiary);
        Task<FamilyFull> FindByFamilyGuidAsync(Guid guid);
        Task<FamilyFull> FindByFamilyGuidAsync(Guid guid, string aggregator, string sequencial);
        Task<FamilyFull> FindByBeneficiaryGuidAsync(Guid hubguid, string aggregator, Guid benefguid);
        Task<FamilyFull> FindByFamilyCardNumberAsync(Guid hubguid, string aggregator, string cardnumber);
        Task<IQueryResult<FamilyFull>> FindByHubGuidAsync(Guid hubguid);
        Task<IQueryResult<FamilyFull>> FindByAggregatorAsync(Guid hubguid, string aggregator);
        Task<IQueryResult<FamilyFull>> FindWithinAggregatorAsync(Guid hubguid, string aggregator);
        Task<IQueryResult<FamilyFull>> FindByEmployeeGuidAsync(Guid hubguid, Guid employeeguid);
        Task<FamilyFull> FindByEmployeeGuidAsync(Guid hubguid, string aggregator, Guid employeeguid);
        Task<IQueryResult<FamilyFull>> FindByContractGuidAsync(Guid hubguid, Guid provider, List<string> contract);
        Task<IQueryResult<FamilyFull>> FindByContractAsync(Guid hubguid, string aggregator, Guid provider, string contract);
        Task<bool> FindActiveCPFAsync(Guid guid, string aggregator, string cpf);
        Task<FamilyDb> FindFamilyDBByEmployeeGuidAsync(Guid hubguid, string aggregator, Guid employeeguid);
        Task<FamilyDb> FindFamilyDBByFamilyGuidAsync(Guid guid);
    }
}
