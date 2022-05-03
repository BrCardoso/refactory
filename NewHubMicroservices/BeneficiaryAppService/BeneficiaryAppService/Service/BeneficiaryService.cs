using BeneficiaryAppService.Converters;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using BeneficiaryAppService.Repository.Interfaces;
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
    public class BeneficiaryService : IBeneficiaryService
    {
        private readonly IBucket bucket;
        private readonly IEmployeeRequestService employeeRequest;
        private readonly IFamilyRepository familyRepository;
        private readonly IPersonRepository personRepository;
        private readonly IProviderRequestService providerRequest;
        private readonly IQueueRequestService queueRequest;
        private readonly IEventRequestService eventRequest;
        private readonly INITService nITService;
        private readonly IRulesConfigurationService rulesconfigurationService;
        private readonly IMandatoryRulesService mandatoryRulesService;
        private readonly IMandatoryRulesRequestService mandatoryRulesRequestService;
        private readonly ITaskPanelRequestService taskPanelRequestService;
        private readonly IMandatoryRulesRuleValidationService mandatoryRulesRuleValidationService;


        public BeneficiaryService(IBucketProvider bucketProvider,
                                IEmployeeRequestService employee,
                                IFamilyRepository family,
                                IPersonRepository person,
                                IProviderRequestService provider,
                                IQueueRequestService queue,
                                IEventRequestService eventR,
                                INITService nIT,
                                IMandatoryRulesService mandatory,
                                IRulesConfigurationService rules,
                                IMandatoryRulesRequestService _mandatoryRulesRequestService,
                                ITaskPanelRequestService _taskPanelRequestService,
                                IMandatoryRulesRuleValidationService _mandatoryRulesRuleValidationService
                                )
        {
            bucket = bucketProvider.GetBucket("DataBucket001");
            employeeRequest = employee;
            familyRepository = family;
            personRepository = person;
            providerRequest = provider;
            queueRequest = queue;
            eventRequest = eventR;
            nITService = nIT;
            rulesconfigurationService = rules;
            mandatoryRulesService = mandatory;
            mandatoryRulesRequestService = _mandatoryRulesRequestService;
            taskPanelRequestService = _taskPanelRequestService;
            mandatoryRulesRuleValidationService = _mandatoryRulesRuleValidationService;
        }

        public async Task<MethodFeedback> UpsertBenefit(TaskResultModel model, string authorization)
        {
            try
            {
                var familyObj = await familyRepository.FindByBeneficiaryGuidAsync(model.hubguid, model.aggregator, model.personguid);
                if (familyObj == null) return new MethodFeedback
                {
                   Success = false,
                   Exception = false,
                   Message = "Familia do beneficiario não localizada"
                };

                int familyMemberIndex = familyObj.family.FindIndex(x => x.personguid == model.personguid);

                if (model.movType == MovimentTypeEnum.INCLUSÃO)
                {
                    if (familyObj.family[familyMemberIndex].Benefitinfos == null) familyObj.family[familyMemberIndex].Benefitinfos = new List<Benefitinfo>();
                    familyObj.family[familyMemberIndex].Benefitinfos.Add(model.benefitinfos);
                }

                PersonDB person = await personRepository.FindByPersonGuidAsync(model.personguid);
                PersonDB employee = await personRepository.FindByPersonGuidAsync(familyObj.personguid);
                EmployeeInfo employeeInfo = await employeeRequest.getInfo(model.hubguid, model.aggregator, familyObj.personguid, authorization);
                BeneficiaryIn beneficiario = familyObj.family[familyMemberIndex];

                var enviou = await nITService.SendAsync(model.hubguid, model.aggregator, model.benefitinfos, model.movType, person, employee, employeeInfo, beneficiario.Typeuser, beneficiario.Kinship, authorization);

                MethodFeedback mf = new MethodFeedback();
                if (enviou.Success)
                {
                    bucket.MutateIn<dynamic>(familyObj.guid.ToString()).Upsert("family", familyObj.family).Execute();
                }
                else
                {
                    mf.Message = enviou.Message;
                    mf.obj = enviou.obj;
                    mf.Success = false;
                }

                return mf;

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<MethodFeedback> SolCard(CardReissue cr, string authorization)
        {
            MethodFeedback mf = new MethodFeedback();
            try
            {
                //busca o cadastro da pessoa
                var findFamily = await familyRepository.FindByFamilyGuidAsync(cr.familyguid);
                var findEmployee = await personRepository.FindByPersonGuidAsync(findFamily.personguid);
                var findPerson = personRepository.FindByPersonGuidAsync(cr.personguid);
                var findEmployeeInfo = employeeRequest.getInfo(cr.hubguid, findFamily.aggregator, findEmployee.Guid, authorization);
                var findProviderStruc = providerRequest.GetProviderStruc(cr.hubguid, cr.aggregator, cr.providerguid, authorization);
                await Task.WhenAll(findPerson, findEmployeeInfo, findProviderStruc);

                var ben = findFamily.family.Where(x => x.personguid == findPerson.Result.Guid).FirstOrDefault();
                var benHolder = findFamily.family.Where(x => x.personguid == findPerson.Result.Guid).FirstOrDefault();
                var benBenefit = benHolder.Benefitinfos.Where(x => x.providerguid == cr.providerguid && x.productcode == cr.providerproductcode).FirstOrDefault();
                if (findPerson != null)
                {
                    QueueModel queue = new QueueModel
                    {
                        Hubguid = cr.hubguid,
                        ProviderGuid = cr.providerguid,
                        Aggregator = cr.aggregator,
                        MovementType = MovimentTypeEnum.SEGUNDA_VIA_CARTEIRINHA,
                        Status = "Aberto",
                        Incdate = DateTime.Now,
                        LinkInformation = new LinkInformation
                        {
                            ContractNumber = benBenefit.contractnumber,
                        },
                        Beneficiary = new List<QueueBeneficiary>(),
                        Charge = null
                    };
                    QueueBeneficiary qb = new QueueBeneficiary
                    {
                        personguid = findPerson.Result.Guid,
                        TypeUser = ben.Typeuser,
                        Kinship = ben.Kinship,
                        Holder = new Holder
                        {
                            personguid = findEmployee.Guid,
                            CardNumber = benBenefit.cardnumber,
                            Cpf = findEmployee.Cpf,
                            Name = findEmployee.Name,
                            JobInfo = new EmployeeinfoClean
                            {
                                Admissiondate = findEmployeeInfo.Result.Admissiondate,
                                Costcenter = findEmployeeInfo.Result.Costcenter,
                                Costcentercode = findEmployeeInfo.Result.Costcentercode,
                                Department = findEmployeeInfo.Result.Department,
                                Departmentcode = findEmployeeInfo.Result.Departmentcode,
                                Employeecomplementaryinfos = findEmployeeInfo.Result.Employeecomplementaryinfos,
                                Functionalcategory = findEmployeeInfo.Result.Functionalcategory,
                                Functionalcategorycode = findEmployeeInfo.Result.Functionalcategorycode,
                                Occupation = findEmployeeInfo.Result.Occupation,
                                Occupationcode = findEmployeeInfo.Result.Occupationcode,
                                Registration = findEmployeeInfo.Result.Registration,
                                Role = findEmployeeInfo.Result.Role,
                                Rolecode = findEmployeeInfo.Result.Rolecode,
                                Salary = findEmployeeInfo.Result.Salary,
                                Shift = findEmployeeInfo.Result.Shift,
                                Union = findEmployeeInfo.Result.Union,
                                Unioncode = findEmployeeInfo.Result.Unioncode
                            }
                        },
                        BenefitInfos = new Benefitinfo
                        {
                            providerproductCode = cr.providerproductcode,
                            cardnumber = benBenefit.cardnumber,
                            startdate = DateTime.Now,
                            ReissueReason = cr.reissuereason
                        }
                    };
                    queue.Beneficiary.Add(qb);


                    //enviar
                    var ret = await queueRequest.Post(queue, authorization);
                    if (ret.Success)
                        //grava nos eventos
                        _ = eventRequest.CreateEventAsync(BenefityToEventConverter.Parse(cr, new Copy2ndRequestEvent
                        {
                            cardNumber = benBenefit.cardnumber,
                            dateRequest = DateTime.Now,
                            Product = cr.product,
                            reissueReason = cr.reissuereason,
                            personGuid = findPerson.Result.Guid,
                            providerGuid = cr.providerguid,
                            providerProductCode = cr.providerproductcode,
                            providerName = cr.providerName,
                            personName = cr.personName
                        }), authorization);

                    return ret;
                }
                else
                {
                    //erro na solicitação
                    mf.Success = false;
                    mf.Message = "Cadastro da pessoa não localizado";
                    return mf;
                }
            }
            catch (Exception ex)
            {
                mf.Success = false;
                mf.Message = ex.ToString();
                mf.Exception = true;
            }
            return mf;
        }

        public async Task<FamilyOut> UpsertFamilyAsync(FamilyIn inputFamily, Guid hubguid, string aggregator, string authorization)
        {
            try
            {
                var Employee = inputFamily.family.Where(e => e.Typeuser.ToUpper() == "TITULAR").SingleOrDefault();
                List<string> messageCode = new List<string>();
                PersonDB personEmployeeDB = null;
                EmployeeDB employeeInfoDB = null;
                FamilyDb familyDb = await familyRepository.FindFamilyDBByFamilyGuidAsync(inputFamily.guid);

                //VARRE A FAMILIA PARA FAZER AS DEVIDAS ATUALIZAÇÕES NO PERSON
                foreach (var inputBeneficiary in inputFamily.family)
                {
                    PersonDB personBeneficiaryDB;
                    if (inputBeneficiary.Typeuser.ToUpper() == "TITULAR")
                    {
                        ///TENTA LOCALIZAR DADOS DO FUNCIONARIO
                        if (Employee.employeeinfo != null && !string.IsNullOrEmpty(Employee.employeeinfo?.Registration))
                        {
                            employeeInfoDB = await employeeRequest.getInfoByRegistration(hubguid, aggregator, Employee.employeeinfo.Registration, authorization);
                            if (employeeInfoDB != null)
                                Employee.personguid = employeeInfoDB.employees[0].personguid;
                        }

                        ////FAZ TRABALHO DE VALIDAR ALTERAÇÕES DO PERSON E SALVAR NO DB////    
                        personBeneficiaryDB = personEmployeeDB = await UpsertPersonAsync((PersonDB)Employee);

                        if (personEmployeeDB == null)
                            return new FamilyOut { Success = false, HttpStatusCode = 500, Message = "Erro ao cadastrar informações do funcionario na base" };
                        inputFamily.personguid = Employee.employeeinfo.personguid = personEmployeeDB.Guid; Employee.Changes = personEmployeeDB.Changes;
                    }
                    else
                    {
                        //LOCALIZA CADASTRO DO PERSON DO FUNCIONARIO TITULAR E EMPLOYEEINFO
                        personEmployeeDB = await personRepository.FindByPersonGuidAsync(inputFamily.personguid);
                        if (employeeInfoDB == null)
                            employeeInfoDB = await employeeRequest.getInfoByPersonGuid(hubguid, aggregator, inputFamily.personguid, authorization);
                        if (employeeInfoDB == null)
                            return new FamilyOut { Success = false, HttpStatusCode = 500, Message = "Erro ao tentar localizar informações do funcionario na base" };

                        ////FAZ TRABALHO DE VALIDAR ALTERAÇÕES DO PERSON E SALVAR NO DB////  
                        personBeneficiaryDB = await UpsertPersonAsync((PersonDB)inputBeneficiary);
                        if (personBeneficiaryDB == null)
                            return new FamilyOut { Success = false, HttpStatusCode = 500, Message = "Erro ao cadastrar informações do dependente na base" };
                        inputBeneficiary.personguid = personBeneficiaryDB.Guid; inputBeneficiary.Changes = personBeneficiaryDB.Changes;
                    }

                    //LOCALIZA O FAMILY
                    if (familyDb == null && inputFamily.personguid != Guid.Empty)
                        familyDb = await familyRepository.FindFamilyDBByEmployeeGuidAsync(hubguid, aggregator, inputFamily.personguid);
                    if (familyDb == null)
                        familyDb = await familyRepository.FindFamilyDBByEmployeeGuidAsync(hubguid, aggregator, personBeneficiaryDB.Guid);
                    //NOVA FAMILIA
                    if (familyDb == null)
                    {
                        if (Employee == null && inputFamily.personguid == Guid.Empty)
                            return new FamilyOut { Success = false, HttpStatusCode = 400, Message = "Para incluir uma nova familia é obrigatorio enviar os dados do funcionario" };
                        else
                            familyDb = new FamilyDb { hubguid = hubguid, aggregator = aggregator, personguid = personEmployeeDB.Guid, family = new List<BeneficiaryDb>() };
                    }

                    //LOCALIZA BENEFICIARIO NO OBJETO DO FAMILYDB
                    int idxBeneficiario = familyDb.family.FindIndex(x => x.personguid == inputBeneficiary.personguid);
                    if (idxBeneficiario < 0)//novo beneficiario
                    {
                        //cria novo beneficiario que será inputado na familia
                        var newBeneficiary = new BeneficiaryDb
                        {
                            BlockDate = inputBeneficiary.BlockDate,
                            BlockReason = inputBeneficiary.BlockReason,
                            personguid = personBeneficiaryDB.Guid,
                            Kinship = inputBeneficiary.Kinship,
                            Origin = string.IsNullOrEmpty(inputBeneficiary.Origin) ? "API" : inputBeneficiary.Origin,
                            Sequencial = familyDb.family.Count().ToString(),
                            Typeuser = inputBeneficiary.Typeuser
                        };

                        //INCLUI OS BENEFICIOS PARA O BENEFICIARIO
                        //LOOP BENEFICIOS
                        foreach (var inputBenefit in inputBeneficiary.Benefitinfos)
                        {
                            var ext = newBeneficiary.benefitinfos?.FindIndex(x => x.providerguid == inputBenefit.providerguid && x.productcode == inputBenefit.productcode && x.contractnumber == inputBenefit.contractnumber && x.blockdate != inputBenefit.blockdate);

                            if (ext < 0 || ext == null)
                            {    //VALIDAR CHANGES DO BENEFICIARY 
                                var upsertBenfit = await processaBeneficiosAsync(personEmployeeDB, Employee?.employeeinfo == null ? employeeInfoDB.employees[0] : Employee.employeeinfo, personBeneficiaryDB, inputBenefit, inputBeneficiary.Kinship, inputBeneficiary.Typeuser, hubguid, aggregator, MovimentTypeEnum.INCLUSÃO, authorization, newBeneficiary.Origin.ToUpper() == "FILE");
                                if (upsertBenfit.Success)
                                {
                                    messageCode.Add(upsertBenfit.MessageCode);
                                    if (newBeneficiary.benefitinfos == null) newBeneficiary.benefitinfos = new List<Benefitinfo>();
                                    newBeneficiary.benefitinfos.Add(inputBenefit);
                                }
                                else
                                {
                                    return new FamilyOut
                                    {
                                        Message = upsertBenfit.Message,
                                        obj = upsertBenfit.obj,
                                        HttpStatusCode = 200,
                                        Success = false,
                                        MessageCode = upsertBenfit.MessageCode
                                    };
                                }

                            }
                        }
                        familyDb.family.Add(newBeneficiary);
                    }
                    else
                    {
                        if (familyDb.family[idxBeneficiario].Kinship != inputBeneficiary.Kinship)//alteração cadastral
                        {
                            inputBeneficiary.Changes.Add(new Change { Attribut = "Kinship", Date = DateTime.Now, Newvalue = inputBeneficiary.Kinship, Oldvalue = familyDb.family[idxBeneficiario].Kinship, Sync = true });
                            familyDb.family[idxBeneficiario].Kinship = inputBeneficiary.Kinship;
                        }
                        if (familyDb.family[idxBeneficiario].Typeuser != inputBeneficiary.Typeuser)//alteração cadastral
                        {
                            inputBeneficiary.Changes.Add(new Change { Attribut = "Typeuser", Date = DateTime.Now, Newvalue = inputBeneficiary.Typeuser, Oldvalue = familyDb.family[idxBeneficiario].Typeuser, Sync = true });
                            familyDb.family[idxBeneficiario].Typeuser = inputBeneficiary.Typeuser;
                        }

                        //INCLUI OU ATUALIZA OS BENEFICIOS PARA O BENEFICIARIO
                        //LOOP BENEFICIOS
                        foreach (var inputBenefit in inputBeneficiary.Benefitinfos)
                        {
                            int? idxBeneficio = familyDb.family[idxBeneficiario].benefitinfos?.FindIndex(x =>
                                                        x.providerguid == inputBenefit.providerguid
                                                        && x.productcode == inputBenefit.productcode
                                                        && (string.IsNullOrEmpty(x.BlockReason) || x.blockdate == inputBenefit.blockdate)
                            );
                            if (idxBeneficio >= 0)
                            {
                                //VALIDAR CHANGES DO BENEFICIARY 
                                var upsertBenfit = await processaBeneficiosAsync(personEmployeeDB,
                                    Employee?.employeeinfo == null ? employeeInfoDB.employees[0] : Employee.employeeinfo,
                                    personBeneficiaryDB,
                                    inputBenefit,
                                    inputBeneficiary.Kinship,
                                    inputBeneficiary.Typeuser,
                                    hubguid, aggregator,
                                    MovimentTypeEnum.ALTERAÇÃO,
                                    authorization,
                                    inputBeneficiary.Origin.ToUpper() == "FILE");

                                if (upsertBenfit.Success)
                                {
                                    messageCode.Add(upsertBenfit.MessageCode);
                                    familyDb.family[idxBeneficiario].benefitinfos[(int)idxBeneficio] = inputBenefit;
                                }
                                else
                                {
                                    //TODO: EXCLUIR BENEFICIO QUANDO ALTERAÇÃO NO BENEFICIARIO FOR IMPACTANTE?
                                    return new FamilyOut
                                    {
                                        Message = upsertBenfit.Message,
                                        obj = upsertBenfit.obj,
                                        HttpStatusCode = 200,
                                        Success = false,
                                        MessageCode = upsertBenfit.MessageCode
                                    };
                                }
                            }

                            else//novo beneficio
                            {
                                //VALIDAR CHANGES DO BENEFICIARY 
                                var upsertBenfit = await processaBeneficiosAsync(personEmployeeDB, Employee?.employeeinfo == null ? employeeInfoDB.employees[0] : Employee.employeeinfo, personBeneficiaryDB, inputBenefit, inputBeneficiary.Kinship, inputBeneficiary.Typeuser, hubguid, aggregator, MovimentTypeEnum.INCLUSÃO, authorization, inputBeneficiary.Origin.ToUpper() == "FILE");
                                if (upsertBenfit.Success)
                                {
                                    messageCode.Add(upsertBenfit.MessageCode);
                                    if (familyDb.family[idxBeneficiario].benefitinfos == null)
                                        familyDb.family[idxBeneficiario].benefitinfos = new List<Benefitinfo>();
                                    familyDb.family[idxBeneficiario].benefitinfos.Add(inputBenefit);
                                }
                                else
                                {
                                    return new FamilyOut
                                    {
                                        Message = upsertBenfit.Message,
                                        obj = upsertBenfit.obj,
                                        HttpStatusCode = 200,
                                        Success = false,
                                        MessageCode = upsertBenfit.MessageCode
                                    };
                                }
                            }
                        }
                    }
                }

                //UPSERT FAMILY
                var postFamily = await familyRepository.AddAsync(familyDb);

                if (Employee != null)
                {
                    Employee.employeeinfo.familyguid = new Guid(postFamily.Id);

                    //UPSERT EMPLOYEE INFO
                    var postEmployee = await employeeRequest.Post(hubguid, aggregator, Employee.employeeinfo, authorization);
                }
                messageCode.RemoveAll(x => x == "" || x == null);
                return new FamilyOut
                {
                    Success = true,
                    HttpStatusCode = 200,
                    guid = familyDb.guid,
                    MessageCode = messageCode.Count > 0 ? messageCode[0] : "",
                    Message = messageCode.Count > 0 ? "Informações salvas, para concluir o processo de adesão do beneficio junto ao provedor, resolva as pendencias que foram criadas no seu painel de tarefas" : ""
                };

                async Task<MethodFeedback> processaBeneficiosAsync(
                    PersonDB personEmployee,
                    EmployeeInfo employeeInfo,
                    PersonDB personBeneficiary,
                    Benefitinfo benefitinfo,
                    string beneficiaryKinship,
                    string beneficiaryTypeuser,
                    Guid hubguid,
                    string aggregator,
                    MovimentTypeEnum movtype,
                    string authorization,
                    bool notify)
                {
                    MethodFeedback mf = new MethodFeedback();
                    if (string.IsNullOrEmpty(benefitinfo.BlockReason))
                    {
                        //valida pelo configurador de regras
                        var validaRulesConfig = await rulesconfigurationService.Validate(hubguid, aggregator, movtype, personBeneficiary, benefitinfo, employeeInfo, beneficiaryKinship, authorization);
                        if (validaRulesConfig.Success)
                        {
                            //valida pelo mandatoty rules
                            MethodFeedback validaMandatoryRules = null;
                            if (movtype == MovimentTypeEnum.INCLUSÃO)
                                validaMandatoryRules = await new MandatoryRulesAddService(mandatoryRulesRequestService, taskPanelRequestService, employeeRequest, mandatoryRulesRuleValidationService).Validate(hubguid, aggregator, benefitinfo, personBeneficiary, beneficiaryKinship, beneficiaryTypeuser, employeeInfo.personguid, authorization, notify);
                            if (movtype == MovimentTypeEnum.ALTERAÇÃO)
                                validaMandatoryRules = await new MandatoryRulesUpdateService(mandatoryRulesRequestService, taskPanelRequestService, employeeRequest, mandatoryRulesRuleValidationService).Validate(hubguid, aggregator, benefitinfo, personBeneficiary, beneficiaryKinship, beneficiaryTypeuser, employeeInfo.personguid, authorization, notify);

                            if (validaMandatoryRules.Success)
                            {
                                if ((movtype == MovimentTypeEnum.INCLUSÃO && benefitinfo.Sync == true) || movtype == MovimentTypeEnum.ALTERAÇÃO)
                                    return await nITService.SendAsync(hubguid, aggregator, benefitinfo, movtype, personBeneficiary, personEmployee, employeeInfo, beneficiaryKinship.ToUpper() == "TITULAR" ? beneficiaryKinship : "DEPENDENTE", beneficiaryKinship, authorization);
                            }
                            else
                            {
                                mf.Message = "Para concluir a solicitação algumas tarefas devem ser solucionadas no painel de tarefas:";
                                mf.obj = validaMandatoryRules.obj;
                                mf.Success = true;
                                mf.MessageCode = "MANDATORY_RULES_PROBLEM";
                                return mf;
                            }
                        }
                        else
                        {
                            mf.Message = $"O beneficiário {personBeneficiary.Name} não atende as regras especificadas no contrato {benefitinfo.contractnumber} - {validaRulesConfig.Message},e por isso não podemos concluir a sua solicitação";
                            mf.obj = validaRulesConfig.obj;
                            mf.Success = false;
                            mf.MessageCode = "RULES_CONFIGURATION_ALERT";
                            return mf;

                        }
                    }
                    mf.Success = true;
                    return mf;
                }

                async Task<PersonDB> UpsertPersonAsync(PersonDB inputPerson)
                {
                    PersonDB personDB;

                    //localiza person do funcionario
                    if (inputPerson.Guid != Guid.Empty)
                        personDB = await personRepository.FindByPersonGuidAsync(inputPerson.Guid);
                    else
                        personDB = await personRepository.FindByPersonNameAsync(inputPerson.Name, inputPerson.Cpf, inputPerson.Birthdate);

                    //inclusão de person funcionario
                    if (personDB == null)
                    {
                        personDB = inputPerson;
                        var ret = await personRepository.AddAsync(personDB);
                        if (ret.Success)
                            personDB.Guid = new Guid(ret.Id);
                        else //não conseguiu cadastrar funcionario
                            return null;
                    }
                    else
                    {
                        inputPerson.Guid = personDB.Guid;
                    }

                    //valida diferenças entre input do person e person do DB
                    List<Change> changes = inputPerson.DetailedCompare(personDB);
                    if (changes.Count > 0)
                    {
                        if (inputPerson.Changes == null) inputPerson.Changes = new List<Change>();
                        if (personDB.Changes == null) inputPerson.Changes = new List<Change>();
                        inputPerson.Changes.AddRange(changes);

                        //update do person no DB
                        if (!(await personRepository.AddAsync(inputPerson)).Success)//não conseguiu atualizar cadastro do person do funcionario
                            return null;
                    }
                    else
                    {
                        if (inputPerson.documents != null)
                        {
                            _ = personRepository.UpSertDocumentsAsync(inputPerson.Guid, inputPerson.documents);
                        }
                        if (inputPerson.complementaryinfos != null)
                        {
                            _ = personRepository.UpSertComplInfosAsync(inputPerson.Guid, inputPerson.complementaryinfos);
                        }
                        _ = personRepository.Update(inputPerson);

                    }
                    return inputPerson;
                }

            }
            catch (Exception ex)
            {
                return new FamilyOut
                {
                    Success = false,
                    Exception = true,
                    HttpStatusCode = 500,
                    Message = ex.ToString()
                };
            }
        }

        public async Task<List<ValidateByContractModel>> ValidateFamiliesByContract(RulesConfigurationModel RC, string authorization)
        {
            List<ValidateByContractModel> ret = new List<ValidateByContractModel>();
            var result = await familyRepository.FindByContractAsync(RC.hubguid, RC.aggregator, RC.providerguid, RC.contractnumber);
            if (result.Success)
            {
                foreach (var familydoc in result.Rows)
                {
                    var employeeinfo = await employeeRequest.getInfo(RC.hubguid, RC.aggregator, familydoc.personguid, authorization);
                    PersonDB employee = new PersonDB();

                    foreach (var pessoa in familydoc.family)
                    {
                        foreach (var benefit in pessoa.Benefitinfos)
                        {
                            if (benefit.providerguid == RC.providerguid && benefit.contractnumber == RC.contractnumber)
                            {
                                var person = await personRepository.FindByPersonGuidAsync(pessoa.personguid);

                                //salva os dados do funcionario na variavel
                                if (pessoa.personguid == familydoc.personguid)
                                    employee = await personRepository.FindByPersonGuidAsync(pessoa.personguid);

                                var validaRulesConfig = rulesconfigurationService.ValidateRuleConf(
                                    RC.hubguid,
                                    RC.aggregator,
                                    MovimentTypeEnum.INCLUSÃO,
                                    person,
                                    benefit,
                                    employeeinfo,
                                    pessoa.Kinship,
                                    RC);

                                if (validaRulesConfig.Count > 0)
                                {
                                    foreach (var item in validaRulesConfig)
                                    {
                                        int idxError = ret.FindIndex(x => x.ErrorKey == item.Key);
                                        if (idxError < 0)
                                        {
                                            ret.Add(new ValidateByContractModel
                                            {
                                                ErrorKey = item.Key,
                                                ErrorValue = item.Value,
                                                InvalidatedPeople = new List<ValidateByContractModel.invalidatedPeople> {
                                                    new ValidateByContractModel.invalidatedPeople {
                                                    CPF = person.Cpf,
                                                    Name = person.Name,
                                                    Kinship = pessoa.Kinship,
                                                    Employeename = employee.Name,
                                                    EmployeeRegistration = employeeinfo.Registration
                                                    }
                                                }
                                            });
                                        }
                                        else
                                        {
                                            ret[idxError].InvalidatedPeople.Add(new ValidateByContractModel.invalidatedPeople
                                            {
                                                CPF = person.Cpf,
                                                Name = person.Name,
                                                Kinship = pessoa.Kinship,
                                                Employeename = employee.Name,
                                                EmployeeRegistration = employeeinfo.Registration
                                            });
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return ret;
        }

        public async Task<List<FamilyFull>> GetFamiliesFull(IQueryResult<FamilyFull> result)
        {
            List<FamilyFull> ret = new List<FamilyFull>();
            //preenche a pessoa
            foreach (var f in result.Rows)
            {
                FamilyFull ff = new FamilyFull
                {
                    aggregator = f.aggregator,
                    guid = f.guid,
                    hubguid = f.hubguid,
                    personguid = f.personguid,
                    family = new List<BeneficiaryIn>()
                };
                foreach (var pessoa in f.family)
                {
                    ff.family.Add(pessoa);
                    var person = await personRepository.FindByPersonGuidAsync(pessoa.personguid);
                    ff.AddPerson(person);
                }
                ret.Add(ff);
            }

            return ret;
        }

    }
}
