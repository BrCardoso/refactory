using BeneficiaryAppService.Models;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons;
using Commons.Base;
using Commons.Enums;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service
{
    public class TransferenceService : ITransferenceService
    {
        private readonly IRulesConfigurationService rulesConfigurationService;
        private readonly IEmployeeRequestService employeeRequest;
        private readonly IMandatoryRulesService mandatoryRulesService;

        public TransferenceService(IMandatoryRulesService mandatoryRulesService)
        {
            this.mandatoryRulesService = mandatoryRulesService;
        }

        private readonly IMandatoryRulesRequestService mandatoryRulesRequestService;
        private readonly ITaskPanelRequestService taskPanelRequest;
        private readonly IMandatoryRulesRuleValidationService mandatoryRulesRuleValidationService;

        public TransferenceService(IRulesConfigurationService rules,
                                IEmployeeRequestService employee,
                                IMandatoryRulesService mandatory,
                                IMandatoryRulesRequestService mandatoryRulesRequest,
                                ITaskPanelRequestService taskPanel,
                                IMandatoryRulesRuleValidationService mandatoryRulesRuleValidation)
        {
            rulesConfigurationService = rules;
            employeeRequest = employee;
            mandatoryRulesService = mandatory;
            mandatoryRulesRequestService = mandatoryRulesRequest;
            taskPanelRequest = taskPanel;
            mandatoryRulesRuleValidationService = mandatoryRulesRuleValidation;
        }

        public async Task<MethodFeedback> ValidateAsync(QueueModel model, string authorization)
        {
            PersonDB beneficiario = (PersonDB)model.Beneficiary[0];
            Benefitinfo newBenefit = new Benefitinfo
            {
                providerguid = model.ProviderGuid,
                productcode = model.Beneficiary[0].BenefitInfos.Transference.ProviderProductCode,
                contractnumber = model.Beneficiary[0].BenefitInfos.contractnumber,
                Sync = true
            };
            EmployeeInfo employee = await employeeRequest.getInfo(model.Hubguid, model.Aggregator, model.Beneficiary[0].Holder.personguid, authorization);
            //valida pelo configurador de regras
            var validaRulesConfig = await rulesConfigurationService.Validate(
                            model.Hubguid,
                            model.Aggregator,
                            MovimentTypeEnum.TRANSFERENCIA_DE_PLANO,
                            beneficiario,
                            newBenefit,
                            employee,
                            model.Beneficiary[0].Kinship,
                            authorization);

            if (validaRulesConfig.Success)
            {
                //valida pelo mandatoty rules
                var validaMandatoryRules = await new MandatoryRulesTransferenceService(mandatoryRulesRequestService, taskPanelRequest, employeeRequest, mandatoryRulesRuleValidationService)
                    .Validate(
                    model.Hubguid,
                    model.Aggregator,
                    newBenefit,
                    beneficiario,
                    model.Beneficiary[0].Kinship,
                    model.Beneficiary[0].TypeUser,
                    employee.personguid,
                    authorization, true);

                if (validaMandatoryRules.Success)
                {
                    return new MethodFeedback
                    {
                        Success = true,
                        HttpStatusCode = 200
                    };
                }
                else
                {
                    return new MethodFeedback
                    {
                        Message = "Para concluir a solicitação algumas tarefas devem ser solucionadas no painel de tarefas:",
                        obj = validaMandatoryRules.obj,
                        Success = true,
                        MessageCode = "MANDATORY_RULES_PROBLEM",
                        HttpStatusCode = 200
                    };
                }
            }
            else
            {
                return new MethodFeedback
                {
                    Message = $"O beneficiário {beneficiario.Name} não atende as regras especificadas no contrato {model.Beneficiary[0].BenefitInfos.contractnumber} do beneficio {model.ProviderGuid},e por isso não podemos concluir a sua solicitação",
                    obj = validaRulesConfig.obj,
                    Success = false,
                    MessageCode = "RULES_CONFIGURATION_ALERT",
                    HttpStatusCode = 400
                };
            }
        }
    }
}
