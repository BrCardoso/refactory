using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Repository.Interfaces;
using Couchbase;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons.Enums;

namespace BeneficiaryAppService.Repository
{
    public class FamilyRepository : IFamilyRepository
    {
        private readonly IBucket _bucket;
        private readonly IEmployeeRequestService _employeeRequest;
        private readonly INITService _nITService;

        public FamilyRepository(IBucketProvider bucket,
                                IEmployeeRequestService employee,
                                INITService nIT)
        {
            _bucket = bucket.GetBucket("DataBucket001");
            _employeeRequest = employee;
            _nITService = nIT;
        }
        public async Task<IOperationResult<FamilyDb>> UpSertBeneficiary(TaskResultModel model, PersonDB findPerson, PersonDB findEmployee, string authorization)
        {
            try
            {
                //localiza a familia no db
                var find = await FindByBeneficiaryGuidAsync(model.hubguid, model.aggregator, model.personguid);
                if (find != null)
                {
                    //atualiza dados do beneficiario
                    int index = find.family.FindIndex(x => x.personguid == model.personguid);
                    if (model.movType == MovimentTypeEnum.INCLUSÃO)
                    {
                        find.family[index].Benefitinfos.Add(model.benefitinfos);
                    }

                    EmployeeInfo findEmployeeInfo = await _employeeRequest.getInfo(model.hubguid, model.aggregator, find.personguid, authorization);
                    BeneficiaryIn beneficiario = (BeneficiaryIn)find.family[index];

                    var enviou = await _nITService.SendAsync(model.hubguid, model.aggregator, model.benefitinfos, model.movType, findPerson, findEmployee, findEmployeeInfo, beneficiario.Typeuser, beneficiario.Kinship, authorization);
                    if (enviou.Success)
                    {
                        var result = _bucket.MutateIn<dynamic>(find.guid.ToString()).Upsert("family", find.family).Execute();
                    }

                    return null;
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
        public async Task<IOperationResult<FamilyDb>> AddAsync(FamilyDb newDocument)
        {
            if (newDocument.guid == Guid.Empty) newDocument.guid = Guid.NewGuid();
            return await UpSert(newDocument);
        }
        public async Task<IOperationResult<FamilyDb>> UpSert(FamilyDb document)
        {
            document.docType = "Family";
            var result = await _bucket.UpsertAsync(document.guid.ToString(), document);
            return result;
        }
        public bool AppendNewBeneficiary(Guid docid, BeneficiaryDb beneficiary)
        {
            try
            {
                var result = _bucket.MutateIn<FamilyDb>(docid.ToString()).
                           ArrayAppend("family", beneficiary).
                           Execute();
                return result.Success;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<FamilyFull> FindByFamilyGuidAsync(Guid guid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.guid= $guid;")
                .AddNamedParameter("$guid", guid)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            var result = await _bucket.QueryAsync<FamilyFull>(query);

            return result.SingleOrDefault();
        }
        public async Task<FamilyFull> FindByFamilyGuidAsync(Guid guid, string aggregator, string sequencial)
        {
            var query = new QueryRequest()
                           .Statement(@"
SELECT t.*, ARRAY s FOR s IN t.family WHEN s.sequencial = $seq END AS family
FROM DataBucket001 AS t
WHERE t.docType = 'Family'
and t.guid = $guid 
and t.aggregator = $aggregator;
")
                           .AddNamedParameter("$guid", guid)
                           .AddNamedParameter("$seq", sequencial)
                           .AddNamedParameter("$aggregator", aggregator)
                           .Metrics(false);

            var result = await _bucket.QueryAsync<FamilyFull>(query);
            return result.SingleOrDefault();
        }
        public async Task<FamilyFull> FindByBeneficiaryGuidAsync(Guid hubguid, string aggregator, Guid benefguid)
        {
            var query = new QueryRequest()
                           .Statement(@"
SELECT t.*
FROM DataBucket001 AS t
unnest t.family as f
WHERE t.docType = 'Family'
    AND t.hubguid = $hubguid
    and f.personguid = $personguid;
")
                           .AddNamedParameter("$hubguid", hubguid)
                           .AddNamedParameter("$aggregator", aggregator)
                           .AddNamedParameter("$personguid", benefguid)
                           .Metrics(false);

            var result = await _bucket.QueryAsync<FamilyFull>(query);
            return result.SingleOrDefault();
        }
        public async Task<FamilyFull> FindByFamilyCardNumberAsync(Guid hubguid, string aggregator, string cardnumber)
        {
            var query = new QueryRequest()
                           .Statement(@"
SELECT t.*
FROM DataBucket001 AS t
unnest t.family as f
unnest f.benefitinfos as b
WHERE t.docType = 'Family'
    AND t.hubguid = $hubguid
    and b.cardnumber = $cardnumber;
")
                           .AddNamedParameter("$hubguid", hubguid)
                           .AddNamedParameter("$cardnumber", cardnumber)
                           .Metrics(false);

            var result = await _bucket.QueryAsync<FamilyFull>(query);
            return result.SingleOrDefault();
        }
        public async Task<IQueryResult<FamilyFull>> FindByHubGuidAsync(Guid hubguid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.hubguid= $hubguid;")
                .AddNamedParameter("$hubguid", hubguid)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            return await _bucket.QueryAsync<FamilyFull>(query);
        }
        public async Task<IQueryResult<FamilyFull>> FindByAggregatorAsync(Guid hubguid, string aggregator)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.hubguid= $hubguid AND d.aggregator= $aggregator ;")
                .AddNamedParameter("$hubguid", hubguid)
                .AddNamedParameter("$aggregator", aggregator)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            return await _bucket.QueryAsync<FamilyFull>(query);
        }
        public async Task<IQueryResult<FamilyFull>> FindWithinAggregatorAsync(Guid hubguid, string aggregator)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.hubguid= $hubguid AND d.aggregator like $aggregator ;")
                .AddNamedParameter("$hubguid", hubguid)
                .AddNamedParameter("$aggregator", aggregator + '%')
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            return await _bucket.QueryAsync<FamilyFull>(query);
        }
        public async Task<IQueryResult<FamilyFull>> FindByEmployeeGuidAsync(Guid hubguid, Guid employeeguid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.hubguid= $hubguid AND d.personguid= $personguid ;")
                .AddNamedParameter("$hubguid", hubguid)
                .AddNamedParameter("$personguid", employeeguid)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            return await _bucket.QueryAsync<FamilyFull>(query);
        }
        public async Task<FamilyFull> FindByEmployeeGuidAsync(Guid hubguid, string aggregator, Guid employeeguid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.hubguid= $hubguid AND d.personguid= $personguid and d.aggregator = $aggregator ;")
                .AddNamedParameter("$hubguid", hubguid)
                .AddNamedParameter("$aggregator", aggregator)
                .AddNamedParameter("$personguid", employeeguid)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);
            var result = await _bucket.QueryAsync<FamilyFull>(query);
            if (result.Success && result.Rows.Count > 0)
                return result.SingleOrDefault();
            return null;
        }
        public async Task<IQueryResult<FamilyFull>> FindByContractGuidAsync(Guid hubguid, Guid provider, List<string> contract)
        {
            var query = new QueryRequest()
                      .Statement(@"SELECT DISTINCT t.*
FROM DataBucket001 AS t
unnest t.family as f
unnest f.benefitinfos as b
WHERE t.docType = 'Family'
    AND t.hubguid = $hubguid
    and b.providerguid = $providerguid
    and b.contractnumber in $contract
")
                      .AddNamedParameter("$hubguid", hubguid)
                      .AddNamedParameter("$providerguid", provider)
                      .AddNamedParameter("$contract", contract)
                      .Metrics(false);

            return await _bucket.QueryAsync<FamilyFull>(query);
        }
        public async Task<IQueryResult<FamilyFull>> FindByContractAsync(Guid hubguid, string aggregator, Guid provider, string contract)
        {
            var query = new QueryRequest()
                      .Statement(@"SELECT DISTINCT t.*
FROM DataBucket001 AS t
unnest t.family as f
unnest f.benefitinfos as b
WHERE t.docType = 'Family'
    AND t.hubguid = $hubguid
    AND t.aggregator = $aggregator
    and b.providerguid = $providerguid
    and b.contractnumber = $contract
")
                      .AddNamedParameter("$hubguid", hubguid)
                      .AddNamedParameter("$aggregator", aggregator)
                      .AddNamedParameter("$providerguid", provider)
                      .AddNamedParameter("$contract", contract)
                      .Metrics(false);

            return await _bucket.QueryAsync<FamilyFull>(query);
        }
        public async Task<FamilyDb> FindFamilyDBByEmployeeGuidAsync(Guid hubguid, string aggregator, Guid employeeguid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.hubguid= $hubguid AND d.aggregator= $aggregator AND d.personguid= $personguid ;")
                .AddNamedParameter("$hubguid", hubguid)
                .AddNamedParameter("$aggregator", aggregator)
                .AddNamedParameter("$personguid", employeeguid)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            var result = await _bucket.QueryAsync<FamilyDb>(query);
            return result.SingleOrDefault();
        }
        public async Task<FamilyDb> FindFamilyDBByFamilyGuidAsync(Guid guid)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.* FROM DataBucket001 AS d WHERE d.docType = $docType AND d.guid= $guid;")
                .AddNamedParameter("$guid", guid)
                .AddNamedParameter("$docType", "Family")
                .Metrics(false);

            var result = await _bucket.QueryAsync<FamilyDb>(query);
            return result.SingleOrDefault();
        }
        public async Task<bool> FindActiveCPFAsync(Guid guid, string aggregator, string cpf)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT dd.cpf, f.blockReason FROM DataBucket001 d 
UNNEST d.family as f JOIN DataBucket001 dd ON KEYS f.personguid
where d.docType = 'Family'
and f.blockReason is null
and dd.cpf = $cpf
and d.hubguid = $guid
and d.aggregator = $aggregator ;")
                .AddNamedParameter("$guid", guid)
                .AddNamedParameter("$cpf", cpf)
                .AddNamedParameter("$aggregator", aggregator)
                .Metrics(false);

            var result = await _bucket.QueryAsync<dynamic>(query);
            return result.Rows.Count > 0 ? true : false;
        }
    }
}