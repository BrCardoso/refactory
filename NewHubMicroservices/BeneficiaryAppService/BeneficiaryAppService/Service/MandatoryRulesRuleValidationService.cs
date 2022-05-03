using BeneficiaryAppService.Models;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons;
using Commons.Base;
using Commons.Enums;
using Commons.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service
{
    public class MandatoryRulesRuleValidationService : IMandatoryRulesRuleValidationService
    {
        private readonly IEmployeeRequestService _employeeRequestService;
        public MandatoryRulesRuleValidationService(IEmployeeRequestService employeeRequestService)
        {
            _employeeRequestService = employeeRequestService;
        }

        public async Task<Dictionary<string, object>> ValidateAsync(MandatoryRules rule, Guid hubguid, string aggregator, Benefitinfo beneficio, PersonDB person, string kinship, string typeuser, Guid employeeGuid, string authorization)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            foreach (var cond in rule.Condition)
            {
                if (!await ValidateConditionAsync(cond, hubguid, aggregator, beneficio, person, kinship, typeuser, employeeGuid, authorization))
                    return new Dictionary<string, object>();
            }
            switch (rule.RuleType)
            {
                case RuleTypeEnum.DOCUMENTO:
                    {
                        var doc = person.documents?.Where(x => rule.Answer.Default.Contains(x.type.ToUpper())).ToList();
                        if (doc == null || doc.Count == 0)
                            dict.Add("Rule:" + rule.Guid, $"O upload do documento de {rule.Description} é obrigatório para concluir a ação solicitada.");
                        break;
                    }
                case RuleTypeEnum.EVENTO_DOC:
                    dict.Add("Rule:" + rule.Guid, $"O upload do documento de {rule.Description} é obrigatório  para concluir a ação solicitada.");
                    break;
                case RuleTypeEnum.EVENTO_EMPLOYEEINFO:
                    dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                    break;
                case RuleTypeEnum.EVENTO_PERSONINFO:
                    dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                    break;
                case RuleTypeEnum.INFORMAÇÃO:
                    {
                        if (rule.Location.Type == LocationTypeEnum.BENEFICIO)
                        {
                            var info = beneficio.GetType().GetProperty(rule.Location.Atribut).GetValue(beneficio, null);
                            if (info == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                        }
                        if (rule.Location.Type == LocationTypeEnum.BENEFICIARIO)
                        {
                            if (rule.Location.Atribut.ToUpper() == "KINSHIP")
                            {

                            }
                            else if (rule.Location.Atribut.ToUpper() == "TYPEUSER")
                            {
                                var info = beneficio.GetType().GetProperty(rule.Location.Atribut).GetValue(beneficio, null);
                                if (info == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");

                            }
                            
                        }
                        else if (rule.Location.Type == LocationTypeEnum.COMPLEMENTAR)
                        {
                            var doc = person.complementaryinfos.Where(x => x.type.ToUpper() == rule.Location.Atribut.ToUpper()).SingleOrDefault();
                            if (doc == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                        }
                        else if (rule.Location.Type == LocationTypeEnum.ENDEREÇO)
                        {
                            var info = person.Addresses?[0].GetType().GetProperty(rule.Location.Atribut).GetValue(person.Addresses?[0], null);
                            if (info == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                        }
                        else if (rule.Location.Type == LocationTypeEnum.OBRIGATORIO)
                        {
                            var info = person.GetType().GetProperty(rule.Location.Atribut).GetValue(person, null);
                            if (info == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                        }
                        else if (rule.Location.Type == LocationTypeEnum.FUNCIONARIO)
                        {
                            var employeeInfo = await _employeeRequestService.getInfo(hubguid, aggregator, employeeGuid, authorization);
                            if (employeeInfo != null)
                            {
                                var info = employeeInfo.GetType().GetProperty(rule.Location.Atribut).GetValue(employeeInfo, null);
                                if (info == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                            }
                        }
                        else if (rule.Location.Type == LocationTypeEnum.FUNCIONARIO_COMPL)
                        {
                            var employeeInfo = await _employeeRequestService.getInfo(hubguid, aggregator, employeeGuid, authorization);
                            if (employeeInfo != null)
                            {
                                var info = employeeInfo.Employeecomplementaryinfos.GetType().GetProperty(rule.Location.Atribut).GetValue(employeeInfo.Employeecomplementaryinfos, null);
                                if (info == null) dict.Add("Rule:" + rule.Guid, $"O campo {rule.Description} é obrigatório  para concluir a ação solicitada.");
                            }
                        }

                        break;
                    }
            }

            return dict;
        }

        private async Task<bool> ValidateConditionAsync(MandatoryRules.MRCondition cond, Guid hubguid, string aggregator, Benefitinfo beneficio, PersonDB person, string kinship, string typeuser, Guid employeeGuid, string authorization)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            object info = null;
            switch (cond.Location)
            {
                case LocationTypeEnum.BENEFICIO:
                    {
                        info = beneficio.GetType().GetProperty(cond.Atribut).GetValue(beneficio, null);
                        break;
                    }
                case LocationTypeEnum.BENEFICIARIO:
                    {
                        if (cond.Atribut.ToUpper() =="KINSHIP")
                        {
                            info = kinship;
                            break;
                        }
                        else if (cond.Atribut.ToUpper() == "TYPEUSER")
                        {
                            info = typeuser;
                            break;
                        }
                        break;
                    }

                case LocationTypeEnum.COMPLEMENTAR:
                    {
                        info = person.complementaryinfos.Where(x => x.type.ToUpper() == cond.Atribut.ToUpper()).SingleOrDefault();
                        break;
                    }

                case LocationTypeEnum.ENDEREÇO:
                    {
                        info = person.Addresses?[0].GetType().GetProperty(cond.Atribut).GetValue(person.Addresses?[0], null);
                        break;
                    }

                case LocationTypeEnum.OBRIGATORIO:
                    {
                        info = person.GetType().GetProperty(cond.Atribut).GetValue(person, null);
                        break;
                    }

                case LocationTypeEnum.FUNCIONARIO:
                    {
                        var employeeInfo = await _employeeRequestService.getInfo(hubguid, aggregator, employeeGuid, authorization);
                        info = employeeInfo.GetType().GetProperty(cond.Atribut).GetValue(employeeInfo, null);
                        break;
                    }

                case LocationTypeEnum.FUNCIONARIO_COMPL:
                    {
                        var employeeInfo = await _employeeRequestService.getInfo(hubguid, aggregator, employeeGuid, authorization);
                        info = employeeInfo.Employeecomplementaryinfos.GetType().GetProperty(cond.Atribut).GetValue(employeeInfo.Employeecomplementaryinfos, null);
                        break;
                    }
            }
            if (info == null) return false;
            return ValidaValores(info, cond.Type, cond.Value, cond.ValueType);
        }

        private bool ValidaValores(object info, OperatorEnum type, string value, ValueTypeEnum valueType)
        {
            switch (valueType)
            {
                case ValueTypeEnum.DATE:
                    DateTime valueToCompare; DateTime.TryParse(value, out valueToCompare);
                    DateTime valueField; DateTime.TryParse(info.ToString(), out valueField);
                    return ValidaCampo(valueField, type, valueToCompare);

                case ValueTypeEnum.TEXT:
                    return ValidaCampo(info.ToString(), type, value.ToString());

                case ValueTypeEnum.NUMBER:
                    int valueToCompareN; int.TryParse(value, out valueToCompareN);
                    int valueFieldN; int.TryParse(info.ToString(), out valueFieldN);
                    return ValidaCampo(valueFieldN, type, valueToCompareN);

                case ValueTypeEnum.MOEDA:
                    float valueToCompareF; float.TryParse(value, out valueToCompareF);
                    float valueFieldF; float.TryParse(info.ToString(), out valueFieldF);
                    return ValidaCampo(valueFieldF, type, valueToCompareF);

                case ValueTypeEnum.BOOL:
                    bool valueToCompareB; bool.TryParse(value, out valueToCompareB);
                    bool valueFieldB; bool.TryParse(info.ToString(), out valueFieldB);
                    return ValidaCampo(valueFieldB, type, valueToCompareB);

                default:
                    return false;
            }
        }

        private bool ValidaCampo(bool? valueField, OperatorEnum type, bool? valueToCompare)
        {
            switch (type)
            {
                case OperatorEnum.VAZIO:
                    return valueField == null;
                case OperatorEnum.NAO_VAZIO:
                    return valueField != null;
                case OperatorEnum.DIFERENTE:
                    return valueField != valueToCompare;
                case OperatorEnum.IGUAL:
                    return valueField == valueToCompare;
                case OperatorEnum.VERDADEIRO:
                    return valueField == true;
                case OperatorEnum.FALSO:
                    return valueField == false;
                default:
                    return false;
            }
        }

        private bool ValidaCampo(float? valueField, OperatorEnum type, float? valueToCompare)
        {
            switch (type)
            {
                case OperatorEnum.MAIOR_IGUAL:
                    return valueField >= valueToCompare;
                case OperatorEnum.MENOR_IGUAL:
                    return valueField <= valueToCompare;
                case OperatorEnum.MAIOR:
                    return valueField > valueToCompare;
                case OperatorEnum.MENOR:
                    return valueField < valueToCompare;
                case OperatorEnum.DIFERENTE:
                    return valueField != valueToCompare;
                case OperatorEnum.VAZIO:
                    return valueField == null;
                case OperatorEnum.NAO_VAZIO:
                    return valueField != null;
                case OperatorEnum.IGUAL:
                    return valueField == valueToCompare;
                default:
                    return false;
            }
        }

        private bool ValidaCampo(int? valueField, OperatorEnum type, int? valueToCompare)
        {
            switch (type)
            {
                case OperatorEnum.MAIOR_IGUAL:
                    return valueField >= valueToCompare;
                case OperatorEnum.MENOR_IGUAL:
                    return valueField <= valueToCompare;
                case OperatorEnum.MAIOR:
                    return valueField > valueToCompare;
                case OperatorEnum.MENOR:
                    return valueField < valueToCompare;
                case OperatorEnum.DIFERENTE:
                    return valueField != valueToCompare;
                case OperatorEnum.VAZIO:
                    return valueField == null;
                case OperatorEnum.NAO_VAZIO:
                    return valueField != null;
                case OperatorEnum.IGUAL:
                    return valueField == valueToCompare;
                default:
                    return false;
            }
        }

        private bool ValidaCampo(string valueField, OperatorEnum type, string? valueToCompare)
        {
            valueField = Helpers.RemoveNaoNumericos(valueField);
            valueToCompare = Helpers.RemoveNaoNumericos(valueToCompare);
            switch (type)
            {
                case OperatorEnum.DIFERENTE:
                    return valueField.Trim().ToUpper() != valueToCompare.Trim().ToUpper();
                case OperatorEnum.VAZIO:
                    return valueField == string.Empty;
                case OperatorEnum.NAO_VAZIO:
                    return valueField != string.Empty;
                case OperatorEnum.IGUAL:
                    return valueField.Trim().ToUpper() == valueToCompare.Trim().ToUpper();
                default:
                    return false;
            }
        }

        private bool ValidaCampo(DateTime? valueField, OperatorEnum type, DateTime? valueToCompare)
        {
            switch (type)
            {
                case OperatorEnum.MAIOR_IGUAL:
                    return valueField >= valueToCompare;
                case OperatorEnum.MENOR_IGUAL:
                    return valueField <= valueToCompare;
                case OperatorEnum.MAIOR:
                    return valueField > valueToCompare;
                case OperatorEnum.MENOR:
                    return valueField < valueToCompare;
                case OperatorEnum.DIFERENTE:
                    return valueField != valueToCompare;
                case OperatorEnum.VAZIO:
                    return valueField == DateTime.MinValue || valueField == null;
                case OperatorEnum.NAO_VAZIO:
                    return valueField > DateTime.MinValue;
                case OperatorEnum.IGUAL:
                    return valueField == valueToCompare;
                default:
                    return false;
            }
        }
    }
}
