using chargeAppServices.Models;
using ChargeAppServices.Business.Interface;
using ChargeAppServices.Extensions;
using ChargeAppServices.Repository.Interface;
using ChargeAppServices.ServiceRequest.Interface;
using Microsoft.Extensions.Configuration;
using NotifierAppService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.AccessControl;
using System.Threading.Tasks;

namespace ChargeAppServices.Business
{
    public class CreditChargeBusiness : ICreditChargeBusiness
    {
        private readonly IFamilyRequest _familyRequest;
        private readonly IEmployeeInfoRequest _employeeInfoRequest;
        private readonly IHolydaysRepository _holydaysRepository;
        private readonly ICreditChargeRepository _creditChargeRepository;
        private readonly IConfiguration _config;
        public CreditChargeBusiness(IFamilyRequest familyRequest, IEmployeeInfoRequest employeeInfoRequest, IHolydaysRepository holydaysRepository, ICreditChargeRepository creditChargeRepository, IConfiguration configuration)
        {
            _config = configuration;
            _familyRequest = familyRequest;
            _employeeInfoRequest = employeeInfoRequest;
            _holydaysRepository = holydaysRepository;
            _creditChargeRepository = creditChargeRepository;
        }
        public async Task<ChargeOrder> CalculaFoodAsync(List<rulesConfigurationModel> rulesConfigurationList, string authorization)
        {
            var modelNotifierList = new List<Notifier>();
            var modelCharge = new ChargeOrder();
            foreach (var currRule in rulesConfigurationList)
            {
                //Calcula a data da carga
                DateTime dDataCarga = DataDaCarga.Get(currRule.foodrules[0].Daymonthtocharge);

                //Data para geração automatica de calculo
                DateTime dDataAntesCarga = dDataCarga.AddDays(-currRule.foodrules[0].AlertHowManyDaysBefore);

                //Se for a data para geração, ele executa
                if (currRule.foodrules[0].Calculatedbyhub && dDataAntesCarga == DateTime.Today)
                {
                    var nValorCalculado = 0.0;

                    //Pega toda as informações dos funcionarios da empresa por aggregator
                    var findEmployeeInfo = await _employeeInfoRequest.Get(currRule.hubguid, currRule.aggregator, authorization);

                    //Seta valor da competencia no formato yyyy-MM
                    var cCompetencia = $"{dDataCarga.Year}-{dDataCarga.Month.ToString("00")}";

                    //varre os tipo de beneficios cadastrados
                    foreach (var oGeneralRules in currRule.generalproductrulesFood)
                    {
                        var Charges = new List<ChargeOrder.ChargeElement>();
                        var RequestReturn = new ChargeOrder.RequestreturnLs()
                        {
                            Status = Commons.Enums.HubMovementStatus.PendenteConfirmacaoCliente, //Padrao status 0
                            Description = "",
                        };

                        var cTipo = oGeneralRules.type.ToUpper();
                        nValorCalculado = 0;

                        //busca os calculos que possam ter ja craidos na base para aquela competencia
                        var oCargasDuplicadas = await _creditChargeRepository.DuplicateAsync(currRule.hubguid,
                                                                           currRule.providerguid,
                                                                           currRule.guid,
                                                                           currRule.aggregator,
                                                                           cCompetencia,
                                                                           cTipo);
                        if (oCargasDuplicadas != null)
                        {
                            modelCharge = oCargasDuplicadas;
                            Charges.AddRange(oCargasDuplicadas.Charges);
                        }

                        //Busca as familias daquela empresa e aggregator
                        var families = await _familyRequest.Get(currRule.hubguid, currRule.aggregator, authorization);
                        if (families != null)
                            foreach (var EmployeeFamily in families)
                            {
                                //localiza o titular (só o titular pode ter VR e VA)
                                var BeneficiaryEmployee = EmployeeFamily.family.Where(x => x.BlockDate == null && x.Typeuser.ToUpper() == "TITULAR").SingleOrDefault();
                                if (BeneficiaryEmployee != null)
                                {
                                    var currEmployeeInfo = findEmployeeInfo.employees.Where(x => x.personguid == BeneficiaryEmployee.PersonGuid).SingleOrDefault();
                                    if (currEmployeeInfo != null)
                                    {
                                        var beneficio = BeneficiaryEmployee.Benefitinfos?.Where(x => x.BlockDate == null && x.Productcode.ToUpper() == cTipo).SingleOrDefault();
                                        if (beneficio != null && Elegibilidade.Verifica(currEmployeeInfo, oGeneralRules))
                                        {
                                            if (cTipo == "VR")
                                                if (dDataCarga >= currEmployeeInfo.Admissiondate.AddMonths(currRule.foodrules[0].Employeeentry))
                                                    if (currRule.foodrules[0].FixedOrWorkingDays.ToUpper() == "FIXO" || currRule.foodrules[0].FixedOrWorkingDays.ToUpper() == "FIXOS")
                                                        nValorCalculado = currRule.foodrules[0].QuantityOfFixedDays * oGeneralRules.benefitValue;
                                                    else if (currRule.foodrules[0].FixedOrWorkingDays.ToUpper() == "UTEIS" || currRule.foodrules[0].FixedOrWorkingDays.ToUpper() == "ÚTEIS")
                                                    {
                                                        var result3 = await _holydaysRepository.Get();
                                                        if (result3 != null)
                                                        {
                                                            var nDiasUteis = DiasUteis.Calcula(dDataCarga, currRule.foodrules[0].Workingdays, currRule.foodrules[0].DiscountsHolidays, result3);
                                                            nValorCalculado = Convert.ToSingle(nDiasUteis) * oGeneralRules.benefitValue;
                                                        }
                                                    }
                                                    else if (cTipo == "VA")
                                                        nValorCalculado = oGeneralRules.benefitValue;

                                            // Se houver valor calculado, deve-se gravar a lista de Pedido de Carga
                                            if (nValorCalculado > 0)
                                            {
                                                var listCharge = new ChargeOrder.ChargeElement(
                                                    BeneficiaryEmployee.PersonGuid,
                                                    BeneficiaryEmployee.Name, 
                                                    BeneficiaryEmployee.Cpf,
                                                    beneficio.Cardnumber, 
                                                    nValorCalculado, 
                                                    nValorCalculado, 
                                                    DateTime.Now, 
                                                    RequestReturn);

                                                Charges.Add(listCharge);
                                            }

                                            if (Charges.Count > 0)
                                            {
                                                var oCharges = oCargasDuplicadas?.Charges.Where(x => x.Personguid == BeneficiaryEmployee.PersonGuid);
                                                if (oCharges == null || oCharges?.ToList().Count == 0)
                                                {
                                                    var a = _creditChargeRepository.Upsert(new ChargeOrder
                                                    {
                                                        Guid = modelCharge.Guid,
                                                        Hubguid = currRule.hubguid,
                                                        Providercustomerguid = currRule.providerguid,
                                                        Rulesconfigurationguid = currRule.guid,
                                                        Aggregator = currRule.aggregator,
                                                        Competence = cCompetencia,
                                                        Subsegcode = cTipo,
                                                        Charges = Charges
                                                    }
                                                    );
                                                    modelCharge = a;

                                                    var modelNotifier = new Notifier()
                                                    {
                                                        hubguid = currRule.hubguid.ToString(),
                                                        aggregator = currRule.aggregator,
                                                        type = "Food",
                                                        title = $"Novo Calculo Food - Competência ({cCompetencia})",
                                                        description = $"Foi executado novo cálculo do segmento Food para a competência de: {cCompetencia}"
                                                    };

                                                    var find = modelNotifierList.Where(x => x.hubguid == modelNotifier.hubguid && x.aggregator == modelNotifier.aggregator).FirstOrDefault();
                                                    if (find == null)
                                                    {
                                                        modelNotifierList.Add(modelNotifier);
                                                    }
                                                }
                                            }
                                        }

                                    }
                                }
                            }


                    }
                }
                if (modelNotifierList.Count > 0)
                {
                    foreach (var modelNotifier in modelNotifierList)
                    {
                        var cReturn = await Commons.operations.PostNotifierAsync(modelNotifier, _config);
                    }
                }
            }
            return modelCharge;
        }

        public List<ChargeOrder> Upsert(List<ChargeOrder> oCharges)
        {
            try
            {
                foreach (var oCharge in oCharges)
                {
                    var a = _creditChargeRepository.Upsert(new ChargeOrder
                    {
                        Guid = oCharge.Guid,
                        Hubguid = oCharge.Hubguid,
                        Providercustomerguid = oCharge.Providercustomerguid,
                        Rulesconfigurationguid = oCharge.Rulesconfigurationguid,
                        Aggregator = oCharge.Aggregator,
                        Competence = oCharge.Competence,
                        Subsegcode = oCharge.Subsegcode,
                        Charges = oCharge.Charges
                    });
                }
                return oCharges;
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                throw;
            }
        }
    }
}
