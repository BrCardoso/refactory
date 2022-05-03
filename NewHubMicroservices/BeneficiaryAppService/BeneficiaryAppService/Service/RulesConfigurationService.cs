using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using BeneficiaryAppService.Service.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons;
using Commons.Base;
using Commons.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BeneficiaryAppService.Service
{
    public class RulesConfigurationService : IRulesConfigurationService
    {
        private readonly IRulesConfigurationRequestService rulesConfigurationRequest;
        public RulesConfigurationService(IRulesConfigurationRequestService rules)
        {
            rulesConfigurationRequest = rules;
        }

        public async Task<MethodFeedback> Validate(Guid hubguid, string aggregator, MovimentTypeEnum tipo, PersonDB person, Benefitinfo benefit, EmployeeInfo employee, string inputkinship, string authorization)
        {
            RulesConfigurationModel RC;
            Dictionary<string, object> dict = new Dictionary<string, object>();
            var ret = new MethodFeedback();
            RC = await rulesConfigurationRequest.Get(hubguid, aggregator, benefit.providerguid, benefit.contractnumber, authorization);

            if (RC == null)
            {
                ret.Success = false;
                ret.Message = "Contrato desconhecido";
                dict.Add("ERRO", "Informações de contrato ou funcionario não encontradas");
                ret.obj = dict;
                return ret;
            }
            
            ret.Message = RC.contractrulename;
            dict = ValidateRuleConf(hubguid, aggregator, tipo, person, benefit, employee, inputkinship, RC);
            ret.obj = dict;

            if (dict.Count > 0)
            {
                ret.Success = false;
            }
            return ret;
        }

        public Dictionary<string, object> ValidateRuleConf(Guid hubguid, string aggregator, MovimentTypeEnum tipo, PersonDB person, Benefitinfo benefit, EmployeeInfo employee, string inputkinship, RulesConfigurationModel RC)
        {
            Dictionary<string, object> dict = new Dictionary<string, object>();
            if (RC.healthrules != null) 
            {
                if (tipo == MovimentTypeEnum.INCLUSÃO || tipo == MovimentTypeEnum.TRANSFERENCIA_DE_PLANO || tipo == MovimentTypeEnum.ALTERAÇÃO)
                {

                    var productRule = RC.products?.Where(x => x.code == benefit.productcode).FirstOrDefault();
                    if (productRule != null)
                        if (productRule.productrulenames.Count > 0)
                        {
                            var elegBoll = false;
                            Dictionary<string, object> dictEleg = new Dictionary<string, object>();

                            foreach (var rule in productRule.productrulenames)
                            {
                                var prodRuleConfig = RC.generalproductruleshealth?.Where(x => x.productrulename == rule).FirstOrDefault();
                                if (prodRuleConfig != null)
                                {
                                    elegBoll = elegibility(employee, elegBoll, dictEleg, prodRuleConfig);

                                }
                            }
                            if (!elegBoll && dictEleg.Count > 0)
                            {
                                foreach (var item in dictEleg)
                                {
                                    dict.Add(item.Key, item.Value);
                                }
                            }
                        }
                    if (productRule == null)
                    {
                        dict.Add("ERRO DE PRODUTO NÃO CONTEMPLADO", $"Produto não faz parte do contrato {benefit.contractnumber}, e por isso não podemos dar continuidade a sua solicitação.");
                        return dict;
                    }
                }

                //valida grau de parentesco qdo não for titular(para todo tipo de movimentação)
                if (inputkinship.ToUpper() != "TITULAR")
                {
                    validateKinship();
                }

                //valida periodo de experiencia do funcionario
                if (tipo == MovimentTypeEnum.INCLUSÃO && inputkinship.ToUpper() == "TITULAR")
                {
                    if ((employee.Admissiondate.AddMonths(RC.healthrules[0].employeeontry)) > DateTime.Now)
                        dict.Add("ERRO PERIODO DE EXPERIENCIA", "Funcionário ainda não cumpriu período de experiencia definido em contrato");

                }

                if (tipo == MovimentTypeEnum.TRANSFERENCIA_DE_PLANO && RC.healthrules[0].minimumperiod > 0)
                {
                    //TODO: VALIDAR tempo minimo de permanencia
                    //TODO: VALIDAR mes de movimentação
                }
            }
            else if (RC.foodrules != null) //regra de food
            {

                //acha a configuração de elegibilidade do produto no configurador de regras
                var productRule = RC.generalproductrulesFood?.Where(x => x.type.ToUpper() == benefit.productcode.ToUpper());
                if (productRule != null)
                {
                    if (inputkinship.ToUpper() == "TITULAR")
                    {
                        var elegBoll = false;
                        Dictionary<string, object> dictEleg = new Dictionary<string, object>();
                        //varre cada regra de elegibilidade que o produto possa ter
                        foreach (var rule in productRule)
                        {
                            switch (rule.elegibilitylimitations.ToUpper())
                            {
                                case "01":
                                case "CARGO":
                                    if (rule.items.Where(x => x.code == employee.Rolecode || x.description.ToUpper() == employee.Role.ToUpper()).FirstOrDefault() == null)
                                        dictEleg.Add("ERRO CARGO REGRA:" + rule.productrulename, "Cargo do funcionário não elegivel para aderir ao benefício");
                                    else
                                        elegBoll = true;
                                    break;

                                case "02":
                                case "DEPARTAMENTO":
                                    if (rule.items.Where(x => x.code == employee.Departmentcode || x.description.ToUpper() == employee.Department.ToUpper()).FirstOrDefault() == null)
                                        dictEleg.Add("ERRO DEPARTAMENTO REGRA:" + rule.productrulename, "Departamento do funcionário não elegivel para aderir ao benefício");
                                    else
                                        elegBoll = true;
                                    break;



                                case "03":
                                case "CENTRODECUSTO":
                                case "CENTRO DE CUSTO":
                                    if (rule.items.Where(x => x.code == employee.Costcentercode || x.description.ToUpper() == employee.Costcenter.ToUpper()).FirstOrDefault() == null)
                                        dictEleg.Add("ERRO CENTRO DE CUSTO REGRA:" + rule.productrulename, "Centro de custo do funcionário não elegivel para aderir ao benefício");
                                    else
                                        elegBoll = true;
                                    break;



                                //case "04":
                                //case "UF":
                                //if (prodRuleConfig.items.Where(x => x.code == employee.Result.Functionalcategorycode || x.description == employee.Result.Functionalcategory).FirstOrDefault() == null)
                                //    if (!prodRuleConfig.elegibilitylimitations.Contains(employee.Result))
                                //        dict.Add("ERRO", "Estado do funcionário não elegivel para aderir ao benefício");
                                //  else
                                //      elegBoll = true;
                                //    break;



                                case "05":
                                case "PROFISSAO":
                                case "PROFISSÃO":
                                    if (rule.items.Where(x => x.code == employee.Occupationcode || x.description.ToUpper() == employee.Occupation.ToUpper()).FirstOrDefault() == null)
                                        dictEleg.Add("ERRO FUNÇÃO REGRA:" + rule.productrulename, "Função do funcionário não elegivel para aderir ao benefício");
                                    else
                                        elegBoll = true;
                                    break;



                                case "06":
                                case "SINDICATO":
                                    if (rule.items.Where(x => x.code == employee.Unioncode || x.description.ToUpper() == employee.Union.ToUpper()).FirstOrDefault() == null)
                                        dictEleg.Add("ERRO SINDICATO REGRA:" + rule.productrulename, "Sindicato do funcionário não elegivel para aderir ao benefício");
                                    else
                                        elegBoll = true;
                                    break;



                                case "07":
                                case "CATEGORIAFUNCIONAL":
                                case "CATEGORIA FUNCIONAL":
                                    if (rule.items.Where(x => x.code == employee.Functionalcategorycode || x.description.ToUpper() == employee.Functionalcategory.ToUpper()).FirstOrDefault() == null)
                                        dictEleg.Add("ERRO CATEGORIA FUNCIONAL REGRA:" + rule.productrulename, "Categoria funcional do funcionário não elegivel para aderir ao benefício");
                                    else
                                        elegBoll = true;
                                    break;


                                default:
                                    break;
                            }
                        }
                        if (!elegBoll && dictEleg.Count > 0)
                        {
                            foreach (var item in dictEleg)
                            {
                                dict.Add(item.Key, item.Value);
                            }
                        }
                    }
                    else
                    {
                        dict.Add("ERRO TIPO DE USUÁRIO:" + inputkinship, string.Format("Beneficio de {0} disponivel apenas para o titular.", benefit.productcode.ToUpper()));
                    }
                }
            }

            return dict;

            Dictionary<string, object> validateKinship()
            {
                var kinship = RC.healthrules[0].kinship.Where(x => x.value.ToUpper() == inputkinship.ToUpper()).FirstOrDefault();
                if (kinship == null)
                    dict.Add("ERRO GRAU DE PARENTESCO", "Grau de parentesco do beneficiário sem cobertura em especificação do contrato");
                else
                {
                    //valida idade limite
                    if (person.complementaryinfos != null && (person.Kinship.ToUpper() == "FILHO(A)" || person.Kinship.ToUpper() == "ENTEADO(A)"))
                    {
                        var college = person.complementaryinfos.Where(x => x.type.ToUpper() == "COLLEGESTUDENT" || x.type.ToUpper() == "COLLEGE STUDENT").FirstOrDefault();
                        if (college != null)
                        {
                            if (college.value == "true")
                            {
                                if (kinship.studentAgeLimit != null && DateTime.Now.AddYears(-(int)kinship.studentAgeLimit) > person.Birthdate)
                                    dict.Add("ERRO IDADE LIMITE ESTUDANTE", "Beneficiário já atingiu idade limite especificada em contrato");
                            }
                            else if (kinship.ageLimit != null && DateTime.Now.AddYears(-(int)kinship.ageLimit) > person.Birthdate)
                            {
                                dict.Add("ERRO IDADE LIMITE NÃO ESTUDANTE", "Beneficiário já atingiu idade limite especificada em contrato");
                            }
                        }
                    }
                }

                return dict;
            }
        }

        private static bool elegibility(EmployeeInfo employee, bool elegBoll, Dictionary<string, object> dictEleg, productruleHealth prodRuleConfig)
        {
            switch (prodRuleConfig.elegibilitylimitations.ToUpper())
            {
                case "01":
                case "CARGO":
                    if (prodRuleConfig.items.Where(x => x.code == employee.Rolecode || x.description.ToUpper() == employee.Role.ToUpper()).FirstOrDefault() == null)
                        dictEleg.Add("ERRO CARGO REGRA:" + prodRuleConfig.productrulename, "Cargo do funcionário não elegivel para aderir ao benefício");
                    else
                        elegBoll = true;
                    break;

                case "02":
                case "DEPARTAMENTO":
                    if (prodRuleConfig.items.Where(x => x.code == employee.Departmentcode || x.description.ToUpper() == employee.Department.ToUpper()).FirstOrDefault() == null)
                        dictEleg.Add("ERRO DEPARTAMENTO REGRA:" + prodRuleConfig.productrulename, "Departamento do funcionário não elegivel para aderir ao benefício");
                    else
                        elegBoll = true;
                    break;



                case "03":
                case "CENTRODECUSTO":
                case "CENTRO DE CUSTO":
                    if (prodRuleConfig.items.Where(x => x.code == employee.Costcentercode || x.description.ToUpper() == employee.Costcenter.ToUpper()).FirstOrDefault() == null)
                        dictEleg.Add("ERRO CENTRO DE CUSTO REGRA:" + prodRuleConfig.productrulename, "Centro de custo do funcionário não elegivel para aderir ao benefício");
                    else
                        elegBoll = true;
                    break;



                //case "04":
                //case "UF":
                //if (prodRuleConfig.items.Where(x => x.code == employee.Result.Functionalcategorycode || x.description == employee.Result.Functionalcategory).FirstOrDefault() == null)
                //    if (!prodRuleConfig.elegibilitylimitations.Contains(employee.Result))
                //        dict.Add("ERRO", "Estado do funcionário não elegivel para aderir ao benefício");
                //  else
                //      elegBoll = true;
                //    break;



                case "05":
                case "FUNCAO":
                case "FUNÇÃO":
                    if (prodRuleConfig.items.Where(x => x.code == employee.Occupationcode || x.description.ToUpper() == employee.Occupation.ToUpper()).FirstOrDefault() == null)
                        dictEleg.Add("ERRO FUNÇÃO REGRA:" + prodRuleConfig.productrulename, "Função do funcionário não elegivel para aderir ao benefício");
                    else
                        elegBoll = true;
                    break;



                case "06":
                case "SINDICATO":
                    if (prodRuleConfig.items.Where(x => x.code == employee.Unioncode || x.description.ToUpper() == employee.Union.ToUpper()).FirstOrDefault() == null)
                        dictEleg.Add("ERRO SINDICATO REGRA:" + prodRuleConfig.productrulename, "Sindicato do funcionário não elegivel para aderir ao benefício");
                    else
                        elegBoll = true;
                    break;



                case "07":
                case "CATEGORIAFUNCIONAL":
                case "CATEGORIA FUNCIONAL":
                    if (prodRuleConfig.items.Where(x => x.code == employee.Functionalcategorycode || x.description.ToUpper() == employee.Functionalcategory.ToUpper()).FirstOrDefault() == null)
                        dictEleg.Add("ERRO CATEGORIA FUNCIONAL REGRA:" + prodRuleConfig.productrulename, "Categoria funcional do funcionário não elegivel para aderir ao benefício");
                    else
                        elegBoll = true;
                    break;


                default:
                    break;
            }

            return elegBoll;
        }
    }
}
