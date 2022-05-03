using Commons;
using Commons.Base;
using Commons.Enums;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Business.Interfaces;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using NotifierAppService.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using static Commons.Base.Nit;

namespace NetCoreJobsMicroservice.Business
{
    public class MovimentQueueBusiness : IMovimentQueueBusiness
    {
        private readonly INITRequestService _nITRequestService;
        private readonly IProviderRequestService _providerRequestService;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IHubCustomerRequestService _hubCustomerRequestService;
        private readonly IChargeRequestService _chargeRequestService;
        private readonly IBeneficiaryRequestService _beneficiaryRequestService;
        private readonly IEmployeeRequestService _employeeRequestService;
        private readonly ICompanyRequestService _companyRequestService;
        private readonly IConfiguration _config;
        private readonly IBucket _bucket;

        public MovimentQueueBusiness(INITRequestService nITRequestService, IProviderRequestService providerRequestService, IRulesConfigurationRepository rulesConfigurationRepository, IHubCustomerRequestService hubCustomerRequestService, IChargeRequestService chargeRequestService, IBeneficiaryRequestService beneficiaryRequestService, IEmployeeRequestService employeeRequestService, ICompanyRequestService companyRequestService, IConfiguration config, IBucketProvider bucketProvider)
        {
            _nITRequestService = nITRequestService;
            _providerRequestService = providerRequestService;
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _hubCustomerRequestService = hubCustomerRequestService;
            _chargeRequestService = chargeRequestService;
            _beneficiaryRequestService = beneficiaryRequestService;
            _employeeRequestService = employeeRequestService;
            _companyRequestService = companyRequestService;
            _config = config;
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }
        public async Task<MethodFeedback> HandleFileAsync(Guid token, QueueModel model, string authorization)
        {
            MethodFeedback mf = new MethodFeedback();
            try
            {
                //busca dados do cliente hub
                var customer = new HubCustomerOut();
                var HubContractRequest = await _hubCustomerRequestService.getHubCustomerAsync(model.hubguid);
                if (HubContractRequest == null)
                {
                    mf.Success = false;
                    mf.Message = "Não localizamos os dados da empresa.";
                    return mf;
                }
                customer = HubContractRequest;

                //busca dados do provedor
                var provider = new ProviderDB();
                var prov = await _providerRequestService.getProviderAsync(model.providerguid);
                if (prov == null)
                {
                    mf.Success = false;
                    mf.Message = "Não localizamos os dados do provedor.";
                    return mf;
                }
                //busca dados do produto
                var produto = prov.Products.Where(x => x.Providerproductcode.ToUpper() == model.beneficiary[0].benefitinfos.providerproductCode.ToUpper()).FirstOrDefault();

                //busca dados da empresa
                var company = await _companyRequestService.getCompanyAsync(model.aggregator.Split("-")[2]);
                if (company == null)
                {
                    mf.Success = false;
                    mf.Message = "Não localizamos os dados do provedor.";
                    return mf;
                }

                //busca dados do provedorStruc
                var findProviderStruc = new ProviderStrucDB();
                var provStruc = await _providerRequestService.getProviderStrucAsync(model.hubguid, model.providerguid, model.aggregator, authorization);
                if (provStruc != null)
                {
                    findProviderStruc = provStruc;
                }

                //busca dados do Contrato
                var findRuleConfig = _rulesConfigurationRepository.getRC(model.hubguid, null, model.providerguid, model.linkinformation.contractnumber);


                //inclui no couchbase
                model.linkinformation = new LinkInformation
                {
                    costumernumber = findProviderStruc.accesscredentials.Costumernumber,
                    login = findProviderStruc.accesscredentials.Login,
                    password = findProviderStruc.accesscredentials.Password,
                    contractissued = findRuleConfig.contractIssued,
                    contractnumber = findRuleConfig.contractnumber,
                    responsibleperson = findRuleConfig.hrresponsable.name,
                    responsibleid = findRuleConfig.hrresponsable.CPF,
                    responsiblebirthdate = findRuleConfig.hrresponsable.birthdate
                };

                if (string.IsNullOrEmpty(model.beneficiary[0].benefitinfos.product))
                {
                    model.beneficiary[0].benefitinfos.product = produto.Description;
                }

                if(string.IsNullOrEmpty(model.beneficiary[0].benefitinfos.subsegment))
                {
                    model.beneficiary[0].benefitinfos.subsegment = produto.Subsegcode;
                }

                if (model.beneficiary[0].holder.jobinfo == null)
                {
                    //busca dados do Funcionario
                    var Employee = await _employeeRequestService.getEmployeeInfoAsync(model.beneficiary[0].holder.personguid, model.hubguid, model.aggregator, authorization);

                    model.beneficiary[0].holder.jobinfo = new EmployeeinfoClean
                    {
                        Admissiondate = Employee.Admissiondate,
                        Costcenter = Employee.Costcenter,
                        Costcentercode = Employee.Costcentercode,
                        Department = Employee.Department,
                        Departmentcode = Employee.Departmentcode,
                        Employeecomplementaryinfos = Employee.Employeecomplementaryinfos,
                        Functionalcategory = Employee.Functionalcategory,
                        Functionalcategorycode = Employee.Functionalcategorycode,
                        Occupation = Employee.Occupation,
                        Occupationcode = Employee.Occupationcode,
                        Registration = Employee.Registration,
                        Role = Employee.Role,
                        Rolecode = Employee.Rolecode,
                        Salary = Employee.Salary,
                        Shift = Employee.Shift,
                        Union = Employee.Union,
                        Unioncode = Employee.Unioncode
                    };
                }

                if (model.guid == Guid.Empty)
                {
                    model.guid = Guid.NewGuid();
                    model.incdate = DateTime.Now;
                    model.status = "Aberto";
                }
                var dbOperation = _bucket.Upsert(
                    model.guid.ToString(), new
                    {
                        model.guid,
                        docType = "Movement",
                        model.incdate,
                        model.hubguid,
                        model.aggregator,
                        model.providerguid,
                        model.movementtype,
                        model.status,
                        model.linkinformation,
                        model.beneficiary,
                        model.charge
                    });
                if (dbOperation.Success)
                {
                    var objAggreg = customer.Hierarchy.Groups.Where(x => x.Companies.Any(y => y.Branches.Contains(company.companyid))).Select(o => new { Group = o, Company = o.Companies.Where(d => d.Branches.Contains(company.companyid)).ToList() }).ToList();

                    string group = objAggreg[0].Group.Name;
                    for (int i = 0; i < model.beneficiary.Count; i++)
                    {
                        var person = await _beneficiaryRequestService.getPersonAsync(model.beneficiary[i].personguid);
                        if (person != null)
                        {
                            model.beneficiary[i].Name = person.Name;
                            model.beneficiary[i].Cpf = person.Cpf;
                            model.beneficiary[i].Birthdate = person.Birthdate;
                            model.beneficiary[i].Gender = person.Gender;
                            model.beneficiary[i].Maritalstatus = person.Maritalstatus;
                            model.beneficiary[i].Mothername = person.Mothername;
                            model.beneficiary[i].Rg = person.Rg;
                            model.beneficiary[i].Issuingauthority = person.Issuingauthority;
                            model.beneficiary[i].complementaryinfos = person.complementaryinfos;
                            model.beneficiary[i].documents = person.documents;
                            model.beneficiary[i].Addresses = person.Addresses;
                            model.beneficiary[i].Phoneinfos = person.Phoneinfos;
                            model.beneficiary[i].emailinfos = person.emailinfos;
                            model.beneficiary[i].ItemStatus = new StatusItemDetails
                            {
                                datetime = DateTime.Now,
                                status = NitStatusTask.Pendente,
                                response = string.Empty
                            };
                        }
                        else
                        {
                            mf.Success = false;
                            mf.Message = "Não localizamos os dados da empresa.";
                            return mf;
                        }
                    }

                    var NITModel = new NitModel
                    {
                        guid = Guid.Empty,
                        nitStatus = new StatusDetails
                        {
                            datetime = DateTime.Now,
                            status = NitStatusTask.Pendente,
                            response = ""
                        },
                        movementtype = model.movementtype,
                        movementguid = model.guid,
                        beneficiary = model.beneficiary,
                        charge = model.charge,
                        linkinformation = model.linkinformation,
                        provider = new Nit.Provider
                        {
                            cnpj = prov.Cnpj,
                            name = prov.Description,
                            providerguid = prov.guid,
                            providerregistrationcode = prov.Registration,
                            segment = prov.Segcode,
                            site = prov.Site
                        },
                        customer = new Customer
                        {
                            address = company.Addresses,
                            aggregator = model.aggregator,
                            branchname = company.Branchname,
                            companyid = company.companyid,
                            companyname = company.TradingName,
                            group = group
                        }
                    };

                    var sendToNit = await _nITRequestService.PostNITAsync(NITModel);
                    if (sendToNit.Success)
                    {
                        var modelNotifier = new Notifier();
                        modelNotifier.hubguid = model.hubguid.ToString();
                        modelNotifier.aggregator = model.aggregator;
                        modelNotifier.type = "Movecad";
                        modelNotifier.title = "Movimentação enviada para o provedor";
                        modelNotifier.description = "Foi enviado uma nova movimentação para o provedor " + provider.Description;

                        var cReturn = await operations.PostNotifierAsync(modelNotifier, _config);
                    }

                    return sendToNit;
                }
                else
                {
                    return new MethodFeedback { Exception = false, Message = dbOperation.Message + " - " + dbOperation.Exception?.ToString(), Success = false };
                }
            }
            catch (Exception ex)
            {

                mf.Success = false;
                mf.Exception = true;
                mf.Message = ex.ToString();
                return mf;
            }
        }
    }
}
