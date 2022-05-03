using Commons;
using Commons.Base;
using Commons.Enums;
using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Business.Interfaces;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using NotifierAppService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Commons.Base.Nit;

namespace NetCoreJobsMicroservice.Business
{
    public class ChargeBusiness : IChargeBusiness
    {
        private readonly INITRequestService _nITRequestService;
        private readonly IProviderRequestService _providerRequestService;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IHubCustomerRequestService _hubCustomerRequestService;
        private readonly IChargeRequestService _chargeRequestService;
        private readonly ICompanyRequestService _companyRequestService;
        private readonly IConfiguration _config;

        public ChargeBusiness(INITRequestService nITRequestService, IProviderRequestService providerRequestService, IRulesConfigurationRepository rulesConfigurationRepository, IHubCustomerRequestService hubCustomerRequestService, IChargeRequestService chargeRequestService, ICompanyRequestService companyRequestService, IConfiguration config)
        {
            _nITRequestService = nITRequestService;
            _providerRequestService = providerRequestService;
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _hubCustomerRequestService = hubCustomerRequestService;
            _chargeRequestService = chargeRequestService;
            _companyRequestService = companyRequestService;
            _config = config;
        }

        /// <summary>
        /// Realizaca operacoes com os pedidos de carga enviados, disponibilizando no NIT
        /// Soon, integracoes serao incluidas neste metodo
        /// </summary>
        /// <param name="token"></param>
        /// <param name="id"></param>
        /// <param name="aggregator"></param>
        /// <param name="authorization"></param>
        /// <returns></returns>
        public async Task<List<MethodFeedback>> HandleChargeAsync(Guid token, Guid id, string aggregator, string authorization)
        {
            List<MethodFeedback> mf = new List<MethodFeedback>();
            try
            {
                //buscar documento de pedido de carga
                var chargeOrder = await _chargeRequestService.getAsync(token, id, aggregator, authorization);
                if (chargeOrder == null)
                {
                    mf.Add(new MethodFeedback
                    {
                        Success = false,
                        Message = "Não localizamos os dados da carga."
                    });
                    return mf;
                }

                List<ChargeBeneficiary> ltBen = new List<ChargeBeneficiary>();

                var customer = new HubCustomerOut();
                var HubContractRequest = await _hubCustomerRequestService.getHubCustomerAsync(token);
                if (HubContractRequest == null)
                {
                    mf.Add(new MethodFeedback
                    {
                        Success = false,
                        Message = "Não localizamos os dados da empresa."
                    });
                    return mf;
                }
                customer = HubContractRequest;

                var group = customer.Hierarchy.Groups.Where(x => x.Code == chargeOrder.Aggregator.Split("-")[0]).FirstOrDefault();

                var provider = new ProviderDB();
                var prov = await _providerRequestService.getProviderAsync(chargeOrder.Providercustomerguid);
                if (prov == null)
                {
                    mf.Add(new MethodFeedback
                    {
                        Success = false,
                        Message = "Não localizamos os dados do provedor."
                    });
                    return mf;
                }
                provider = prov;
                //buscar documento de estrutura de provedor
                var providerStruc = new ProviderStrucDB();
                var provStruc = await _providerRequestService.getProviderStrucAsync(token, chargeOrder.Providercustomerguid, chargeOrder.Aggregator, authorization);
                if (provStruc == null)
                {
                    mf.Add(new MethodFeedback
                    {
                        Success = false,
                        Message = "Não localizamos os dados do provedor na empresa."
                    });
                    return mf;
                }
                providerStruc = provStruc;
                //buscar documento da empresa
                var company = await _companyRequestService.getCompanyAsync(chargeOrder.Aggregator.Split("-")[2]);
                if (company == null)
                {
                    mf.Add(new MethodFeedback
                    {
                        Success = false,
                        Message = "Não localizamos os dados da empresa."
                    });
                    return mf;
                }

                var rulesConfigurations = _rulesConfigurationRepository.getRC(token, chargeOrder.Rulesconfigurationguid, Guid.Empty, "");

                foreach (var item in chargeOrder.Charges)
                {
                    if (item.Requestreturn.Status == HubMovementStatus.GerarPedido)
                    {
                        item.Requestreturn.Status = HubMovementStatus.EmProcessamento;
                        ltBen.Add(new ChargeBeneficiary
                        {
                            chargeinfo = new Chargeinfo
                            {
                                chargedate = DateTime.Now.Date.AddDays(rulesConfigurations.foodrules[0].Daymonthtocharge - DateTime.Now.Day),
                                creditvalue = item.Total,
                                product = chargeOrder.Subsegcode
                            },
                            cpf = item.CPF,
                            idonprovider = item.Card,
                            name = item.Name,
                            personguid = item.Personguid,
                            ItemStatus = new StatusItemDetails
                            {
                                status = NitStatusTask.Pendente, //Primeiro envio para o Nit, pendente
                                datetime = DateTime.Now,
                                response = ""
                            }
                        });
                    }
                }

                //envia pro NIT - envio inicial
                var NITModel = new NitModel
                {
                    guid = Guid.Empty,
                    movementtype = MovimentTypeEnum.PEDIDO_CARGA,
                    movementguid = id,
                    nitStatus = new StatusDetails
                    {
                        datetime = DateTime.Now,
                        status = NitStatusTask.Pendente, //Status pendente de analise
                        response = ""
                    },
                    charge = new charge
                    {
                        beneficiary = ltBen,
                        subsegcode = chargeOrder.Subsegcode
                    },
                    linkinformation = new Nit.LinkInformation
                    {
                        costumernumber = providerStruc.accesscredentials.Costumernumber,
                        login = providerStruc.accesscredentials.Login,
                        password = providerStruc.accesscredentials.Password,
                        contractissued = rulesConfigurations.contractIssued,
                        contractnumber = rulesConfigurations.contractnumber,
                        responsibleperson = rulesConfigurations.hrresponsable.name,
                        responsibleid = rulesConfigurations.hrresponsable.CPF,
                        responsiblebirthdate = rulesConfigurations.hrresponsable.birthdate
                    },
                    provider = new Nit.Provider
                    {
                        cnpj = provider.Cnpj,
                        name = provider.Description,
                        providerguid = provider.guid,
                        providerregistrationcode = provider.Registration,
                        segment = provider.Segcode,
                        site = provider.Site
                    },
                    customer = new Customer
                    {
                        address = company.Addresses,
                        aggregator = chargeOrder.Aggregator,
                        branchname = company.Branchname,
                        companyid = company.companyid,
                        companyname = company.TradingName,
                        group = group.Name
                    }
                };
                var sendToNit = await _nITRequestService.PostNITAsync(NITModel);
                mf.Add(sendToNit);

                if (sendToNit.Success)
                {
                    //Envia notificação de sucesso
                    _ = operations.PostNotifierAsync(new Notifier
                    {
                        hubguid = chargeOrder.Hubguid.ToString(),
                        aggregator = chargeOrder.Aggregator,
                        type = "Food",
                        title = "Pedido enviado para o provedor",
                        description = "Foi enviado um novo pedido para o provedor " + provider.Description
                    }, _config);

                    //atualiza doc do charge
                    _ = _chargeRequestService.UpdateAsync(new List<ChargeOrder>() { chargeOrder }, authorization);
                }
                else
                {
                    //Falha no envio para o NIT
                    foreach (var item in chargeOrder.Charges)
                    {
                        item.Requestreturn.Status = HubMovementStatus.FalhaNoEnvio;
                    }

                    _ = _chargeRequestService.UpdateAsync(new List<ChargeOrder>() { chargeOrder }, authorization);
                }

                return mf;
            }
            catch (Exception ex)
            {
                mf.Add(new MethodFeedback
                {
                    Exception = true,
                    Success = false,
                    Message = ex.ToString()
                });
                return mf;
            }
        }
    }
}
