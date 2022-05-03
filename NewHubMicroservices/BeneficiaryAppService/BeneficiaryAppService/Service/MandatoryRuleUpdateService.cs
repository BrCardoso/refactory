
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons;
using Commons.Base;
using Commons.Enums;
using Commons.Models;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service
{
    public class MandatoryRulesUpdateService : IMandatoryRulesService
    {
        private readonly IMandatoryRulesRequestService _mandatoryRulesRequest;
        private readonly ITaskPanelRequestService _taskPanelRequest;
        private readonly IMandatoryRulesRuleValidationService _mandatoryRulesRuleValidationService;
        private readonly MovimentTypeEnum movimentType = MovimentTypeEnum.ALTERAÇÃO;
        public MandatoryRulesUpdateService(IMandatoryRulesRequestService mandatory, ITaskPanelRequestService task, IEmployeeRequestService employeeRequestService, IMandatoryRulesRuleValidationService mandatoryRulesRuleValidationService)
        {
            _mandatoryRulesRequest = mandatory;
            _taskPanelRequest = task;
            _mandatoryRulesRuleValidationService = mandatoryRulesRuleValidationService;
        }

        public async Task<MethodFeedback> Validate(Guid hubguid, string aggregator, Benefitinfo beneficio, PersonDB person, string kinship, string typeuser, Guid employeeGuid, string authorization, bool notify)
        {
            MethodFeedback mf = new MethodFeedback();
            Dictionary<string, object> dict = new Dictionary<string, object>();
            try
            {
                //busca as regras do provedor
                var getRules = await _mandatoryRulesRequest.getRules(beneficio.providerguid);
                if (getRules == null)
                    return new MethodFeedback
                    {
                        Success = true,
                        Message = "No rules found."
                    };

                //filtra pelo tipo de movimentação
                var ruleList = getRules.Where(x => x.MovimentType.Contains(movimentType)).ToList();
                //Se a busca por regras de validação retornar vazia, encerra a operação com sucesso(não existem regras adicionais para a operadora
                if (ruleList == null)
                    return new MethodFeedback
                    {
                        Success = true,
                        Message = "No rules found."
                    };

                //Inicializa lista de mandatoryrulesguids
                List<Guid> mrGuids = new List<Guid>();
                foreach (MandatoryRules rule in ruleList)
                {
                    if (rule.Kinship.Contains(kinship.ToUpper()) || rule.Kinship.Contains("*"))
                    {
                        dict.Concat(await _mandatoryRulesRuleValidationService.ValidateAsync(rule, hubguid, aggregator, beneficio, person, kinship, typeuser, employeeGuid, authorization));
                    }
                }

                //Se não foi barrado em nhm regra retorna sucess true
                if (mrGuids.Count == 0)
                    return new MethodFeedback
                    {
                        Success = true
                    };

                //cria painel de tarefas para regras que nao foram validadas e retorna as mensagens descritivas
                var ret = await _taskPanelRequest.Post(new TaskPanelModel
                {
                    hubguid = hubguid,
                    aggregator = aggregator,
                    incdate = DateTime.Now,
                    movtype = movimentType.ToString(),
                    providerguid = beneficio.providerguid,
                    origin = "MOVCAD",
                    personguid = person.Guid,
                    status = "ABERTO",
                    subject = "Validação de obrigatoriedade",
                    mandatoryrulesguids = mrGuids,
                    benefitinfos = beneficio,
                    divergence = new Divergence(),
                    beneficiarydetails = new BeneficiaryDetails
                    {
                        birthdate = person.Birthdate,
                        cpf = person.Cpf,
                        gender = person.Gender,
                        kinship = kinship,
                        maritalstatus = person.Maritalstatus,
                        name = person.Name,
                        rg = person.Rg
                    }
                }, notify, authorization);
                return new MethodFeedback
                {
                    Success = false,
                    obj = dict
                };
            }
            catch (Exception ex)
            {
                return new MethodFeedback { Exception = true, Message = ex.ToString(), obj = dict, Success = false };
            }
        }
    }
}
