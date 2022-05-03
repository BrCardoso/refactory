using BeneficiaryAppService.Converters;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Repository.Interfaces;
using BeneficiaryAppService.ServiceRequest.Interfaces;

using Commons;
using Commons.Base;
using Commons.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NotifierAppService.Models;
using System;
using System.Collections.Generic;
using System.IO;
using BeneficiaryAppService.Service.Interfaces;
using System.Threading.Tasks;
using System.Linq;
using BeneficiaryAppService.Models.External;
using BeneficiaryAppService.Service;
using Commons.Enums;

namespace BeneficiaryAppService.Controler
{
    [Route("api/v1/[controller]")]
    public class FamilyController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IFamilyRepository _familyRepository;
        private readonly IPersonRepository _personRepository;
        private readonly IEventRequestService _eventsRequestService;
        private readonly IEmployeeRequestService _employeeRequest;
        private readonly IQueueRequestService _queueRequest;
        private readonly ITransferenceService _transferenceService;
        private readonly IBeneficiaryService _beneficiaryService;
        private readonly INITService _nITService;
        private readonly IRulesConfigurationService _rulesConfigurationService;
        private readonly IBenefitService _benefitService;
        private readonly ITaskPanelRequestService _taskPanelRequest;
        private readonly IMandatoryRulesRequestService _mandatoryRulesRequestService;
        private readonly IMandatoryRulesRuleValidationService _mandatoryRulesRuleValidationService;

        public FamilyController(IFamilyRepository familyRepository,
                                IPersonRepository personRepository,
                                IEventRequestService eventsRequestService,
                                IConfiguration configuration,
                                IEmployeeRequestService employee,
                                IQueueRequestService queue,
                                IBeneficiaryService bene,
                                INITService nIT,
                                ITransferenceService transf,
                                IRulesConfigurationService rules,
                                IBenefitService benefit,
                                ITaskPanelRequestService taskPanelRequest,
                                IMandatoryRulesRequestService mandatoryRulesRequestService,
                                IMandatoryRulesRuleValidationService mandatoryRulesRuleValidationService)
        {
            _config = configuration;
            _familyRepository = familyRepository;
            _personRepository = personRepository;
            _eventsRequestService = eventsRequestService;
            _employeeRequest = employee;
            _queueRequest = queue;
            _transferenceService = transf;
            _beneficiaryService = bene;
            _nITService = nIT;
            _rulesConfigurationService = rules;
            _benefitService = benefit;
            _taskPanelRequest = taskPanelRequest;
            _mandatoryRulesRequestService = mandatoryRulesRequestService;
            _mandatoryRulesRuleValidationService = mandatoryRulesRuleValidationService;
    }


        [HttpGet]
        [Route("Customer/{token}/CheckActiveCPF/{cpf}")]
        public async Task<object> GetAsyncCPF(Guid token, string cpf)
        {
            try
            {
                string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
                var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
                if (!validateUser.Success)
                    return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
                HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

                return Ok(await _familyRepository.FindActiveCPFAsync(token, aggregator, cpf));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}")]
        public async Task<object> GetAsync1(Guid token)
        {
            try
            {
                string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
                var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
                if (!validateUser.Success)
                    return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
                HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

                var result = await _familyRepository.FindByAggregatorAsync(token, aggregator);
                if (!result.Success || result.Rows.Count == 0) //se não achar, procura pelo grupo ou grupo-empresa
                    result = await _familyRepository.FindWithinAggregatorAsync(token, aggregator);

                if (result.Success)//preenche os dados das pesosas no objeto
                {
                    List<FamilyFull> ret = new List<FamilyFull>();
                    List<FamilyFull> servResult = await _beneficiaryService.GetFamiliesFull(result);
                    //ordena por ordem alfabetica do titular
                    servResult = servResult.OrderBy(x => x.family[0].Name).ToList();

                    //quebra familia por usuário
                    foreach (var f in servResult)
                    {
                        foreach (var pessoa in f.family)
                        {
                            FamilyFull ff = new FamilyFull
                            {
                                aggregator = f.aggregator,
                                guid = f.guid,
                                hubguid = f.hubguid,
                                personguid = f.personguid,
                                family = new List<BeneficiaryIn>()
                            };
                            ff.family.Add(pessoa);
                            ret.Add(ff);
                        }
                    }

                    return Ok(ret);
                }
                else
                {
                    return Problem(result.Message?.ToString() + "-" + result.Exception?.ToString());
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }


        [HttpGet]
        [Route("Customer/{token}/Provider/{provider}")]
        public async Task<object> GetAsync(Guid token, Guid provider, List<string> contract)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                for (int i = 0; i < contract.Count; i++)
                {
                    contract[i] = contract[i].Trim();
                }
                var result = await _familyRepository.FindByContractGuidAsync(token, provider, contract);
                if (result.Success)
                {
                    List<FamilyFull> ret = await _beneficiaryService.GetFamiliesFull(result);
                    return Ok(ret);
                }
                else
                {
                    return Problem(result.Message?.ToString() + "-" + result.Exception?.ToString());
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Id/{id}")]
        public async Task<object> Get1Async(Guid token, Guid id)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _familyRepository.FindByFamilyGuidAsync(id);
                if (result.Success)
                {
                    //preenche a pessoa
                    var fam = result;
                    foreach (var pessoa in fam.family)
                    {
                        var person = await _personRepository.FindByPersonGuidAsync(pessoa.personguid);
                        fam.AddPerson(person);
                        if (pessoa.Typeuser?.ToUpper() == "TITULAR" || pessoa.Kinship?.ToUpper() == "TITULAR")
                        {
                            var a = await _employeeRequest.getInfo(fam.hubguid, fam.aggregator, pessoa.personguid, Authorization);
                            pessoa.employeeinfo = a == null ? null : (EmployeeinfoClean)a;
                        }
                    }
                    return Ok(fam);
                }
                else
                {
                    return Problem(result.Message?.ToString());
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Id/{id}/Sequencial/{seq}")]
        public async Task<object> Get2Async(Guid token, Guid id, string seq)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _familyRepository.FindByFamilyGuidAsync(id, aggregator, seq);
                if (result.Success)
                {
                    foreach (var pessoa in result.family)
                    {
                        var person = await _personRepository.FindByPersonGuidAsync(pessoa.personguid);
                        result.AddPerson(person);
                        if (pessoa.Typeuser?.ToUpper() == "TITULAR" || pessoa.Kinship?.ToUpper() == "TITULAR")
                        {
                            var a = await _employeeRequest.getInfo(result.hubguid, result.aggregator, pessoa.personguid, Authorization);
                            pessoa.employeeinfo = a == null ? null : (EmployeeinfoClean)a;
                        }
                    }

                    return Ok(new List<FamilyFull>() { result });
                }
                else
                {
                    return Problem(result.Message?.ToString());
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/CardNumber/{card}")]
        public async Task<object> GetByCardNumberAsync(Guid token, string card)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _familyRepository.FindByFamilyCardNumberAsync(token, aggregator, card);
                if (result != null)
                {
                    //preenche a pessoa
                    var person = await _personRepository.FindByPersonGuidAsync(result.family[0].personguid);
                    result.AddPerson(person);
                    return Ok(result);
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Document/{cpf}/Birth/{date}")]
        public async Task<object> GetByDocumentAsync(Guid token, string cpf, DateTime date)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            FamilyFull fam = new FamilyFull();
            try
            {//localiza pessoa
                var person = await _personRepository.FindByPersonNameAsync(null, cpf, date);
                if (person != null)
                {
                    //localiza a familia
                    var family = await _familyRepository.FindByBeneficiaryGuidAsync(token, aggregator, person.Guid);
                    if (family != null)
                    {
                        fam = family;
                        fam.AddPerson(person);

                        return Ok(fam);
                    }
                    return NotFound();
                }
                return NotFound();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Name/{name}")]
        public async Task<object> GetByNameAsync(Guid token, string name)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            FamilyFull fam = new FamilyFull();
            try
            {//localiza person
                var person = await _personRepository.FindByPersonNameAsync(name, null, null);

                //localiza a familia
                var family = await _familyRepository.FindByBeneficiaryGuidAsync(token, aggregator, person.Guid);
                fam = family;
                fam.AddPerson(person);
                return Ok(fam);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpPost]
        [Route("Customer/{token}")]
        public async Task<object> PostAsync(Guid token, [FromBody] FamilyIn model)
        {
            //using (StreamWriter writetext = new StreamWriter($"{ family.family[0].Cpf}{ DateTime.Now.ToString("yyyyMMddhhmmssffftt")}.txt")) { writetext.WriteLine(JsonConvert.SerializeObject(family)); }
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }

            try
            {
                var currentFamily = await _beneficiaryService.UpsertFamilyAsync(model, token, aggregator, Authorization);
                if (currentFamily.HttpStatusCode == 200)
                {
                    if (string.IsNullOrEmpty(currentFamily.Message))
                        currentFamily.Message = "Os dados foram salvos com sucesso!";
                    return Ok(currentFamily);
                }
                else
                    return StatusCode(currentFamily.HttpStatusCode, currentFamily.Message);
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        [Route("Customer/{token}/FamilyDb")]
        public async Task<object> PostAsync(Guid token, [FromBody] FamilyDb model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            try
            {
                model.hubguid = token;
                model.aggregator = aggregator;
                if (model.guid == Guid.Empty)
                {
                    var findFamily = await _familyRepository.FindByBeneficiaryGuidAsync(model.hubguid, aggregator, model.personguid);
                    if (findFamily != null)
                    {
                        model.guid = findFamily.guid;
                    }
                    else
                    {
                        model.guid = Guid.NewGuid();
                    }
                }
                var a = _familyRepository.UpSert(model);
                return Ok(model);
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        [Route("Customer/{token}/CardReissue")]
        public async Task<object> PostCardAsync(Guid token, [FromBody] CardReissue model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);


            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            try
            {
                model.hubguid = token;
                model.aggregator = aggregator;
                var a = await _beneficiaryService.SolCard(model, Authorization);

                return Ok(a);
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost]
        [Route("Customer/{token}/TaskResult")]
        public async Task<object> PostTaskAsync(Guid token, [FromBody] TaskResultModel model)
        {
            using (StreamWriter writetext = new StreamWriter($"{ model.personguid}{ DateTime.Now.ToString("yyyyMMddhhmmssffftt")}.txt")) { writetext.WriteLine(JsonConvert.SerializeObject(model)); }

            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }

            try
            {
                model.hubguid = token;
                model.aggregator = aggregator;
                var benef = await _familyRepository.FindByBeneficiaryGuidAsync(model.hubguid, model.aggregator, model.personguid);
                PersonDB employee; PersonDB beneficiary;
                if (model.personguid == benef.personguid)
                {
                    employee = _personRepository.FindByPersonGuidAsync(model.personguid).Result;
                    beneficiary = _personRepository.FindByPersonGuidAsync(benef.personguid).Result;
                }
                else
                {
                    beneficiary = employee = await _personRepository.FindByPersonGuidAsync(model.personguid);
                }
                var a = await _beneficiaryService.UpsertBenefit(model, Authorization);
                if (a.Success)
                {
                    //envia notificação pra aplicação exibir no front
                    var modelNotifier = new Notifier()
                    {
                        hubguid = model.hubguid.ToString(),
                        aggregator = aggregator,
                        type = "Painel de tarefas",
                        title = "Informações do usuário atualizadas.",
                        description = $"As pendencias cadastrais do beneficiario foram sanadas e agora daremos continuidada à {model.movType} do beneficio."
                    };
                    var cReturn = await Commons.operations.PostNotifierAsync(modelNotifier, _config);

                    return Ok(a);
                }
                else
                {
                    return Ok(a);
                }
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        [HttpPost("Customer/{token}/BlockBenefit")]
        public async Task<IActionResult> PostBlockBenefitAsync(Guid token, [FromBody] BenefitTransactionModel.BenefitDocument model)
        {
            string aggregator = Request.GetHeader("aggregator");
            string Authorization = Request.GetHeader("Authorization");
            string refresh_token = Request.GetHeader("refresh_token");

            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            model.hubguid = token;
            model.aggregator = aggregator;

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }

            var ret = await _benefitService.blockBenefitAsync(model, Authorization);
            return StatusCode(ret.HttpStatusCode, ret) ;

        }

        [HttpPost("Customer/{token}/BlockBeneficiary")]
        public async Task<IActionResult> PostBlockBeneficiaryAsync(Guid token, [FromBody] BenefitTransactionModel.BenefitDocument model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            try
            {
                model.hubguid = token;
                model.aggregator = aggregator;
                var foundDocument = await _familyRepository.FindByFamilyGuidAsync(model.guid);
                if (!(foundDocument is FamilyFull))
                    return BadRequest("Familia não localizada");

                // foundDocument.file = newDocumentInfo.file;

                var newBlockDate = (DateTime)model.family[0].blockDate;
                var newBlockReason = model.family[0].blockReason;

                var foundEmployee = await _personRepository.FindByPersonGuidAsync(foundDocument.personguid);

                var foundEmployeeInfo = await _employeeRequest.getInfo(model.hubguid, aggregator, foundDocument.personguid, Authorization);

                var beneficiaryidx = foundDocument.family.FindIndex(f => f.personguid == model.family[0].personguid);

                if (foundDocument.family[beneficiaryidx].Typeuser.ToUpper() == "TITULAR")
                {
                    //BLOQUEIA TODA A FAMILIA E TODOS OS BENEFICIOS
                    for (int i = 0; i < foundDocument.family.Count; i++)
                    {
                        //BLOQUEIA TODOS OS BENEFICIOS DE UM UNICO BENEFICIARIO
                        foundDocument.family[i].Benefitinfos = await bloqueiaBeneficiosAsync(
                            foundDocument.family[i].Benefitinfos,
                            newBlockDate,
                            newBlockReason,
                            foundEmployee,
                            foundEmployeeInfo);
                        //bloqueia beneficiario
                        foundDocument.family[i].BlockDate = newBlockDate;
                        foundDocument.family[i].BlockReason = newBlockReason;
                        if (!string.IsNullOrEmpty(model.filetype))
                        {
                            var updDocument = _personRepository.UpSertDocumentsAsync(
                         foundDocument.family[i].personguid,
                         new List<Document>(){
                            new Document{
                                type = model.filetype, image_front = model.file, incdate = DateTime.Now
                                }
                             });
                        }

                    }
                }
                else
                {
                    //BLOQUEIA TODOS OS BENEFICIOS DE UM UNICO BENEFICIARIO
                    foundDocument.family[beneficiaryidx].Benefitinfos = await bloqueiaBeneficiosAsync(
                            foundDocument.family[beneficiaryidx].Benefitinfos,
                            newBlockDate,
                            newBlockReason,
                            foundEmployee,
                            foundEmployeeInfo);
                    //bloqueia beneficiario
                    foundDocument.family[beneficiaryidx].BlockDate = newBlockDate;
                    foundDocument.family[beneficiaryidx].BlockReason = newBlockReason;
                    if (!string.IsNullOrEmpty(model.filetype))
                    {
                        var updDocument = _personRepository.UpSertDocumentsAsync(
                        foundDocument.family[beneficiaryidx].personguid,
                        new List<Document>(){
                            new Document{
                                type = model.filetype, image_front = model.file, incdate = DateTime.Now
                                }
                            });
                    }

                }

                async Task<List<Benefitinfo>> bloqueiaBeneficiosAsync(List<Benefitinfo> benefitinfos, DateTime newBlockDate, string newBlockReason, PersonDB foundEmployee, EmployeeInfo foundEmployeeInfo)
                {
                    if (benefitinfos != null)
                    {
                        for (int i = 0; i < benefitinfos.Count; i++)
                        {
                            benefitinfos[i].blockdate = newBlockDate;
                            benefitinfos[i].BlockReason = newBlockReason;
                            var foundPerson = await _personRepository.FindByPersonGuidAsync(model.family[0].personguid);
                            var sendToNIT = await _nITService.SendAsync(
                                model.hubguid,
                                aggregator,
                                benefitinfos[i],
                                MovimentTypeEnum.EXCLUSÃO,
                                foundPerson,
                                foundEmployee,
                                foundEmployeeInfo,
                                foundDocument.family[beneficiaryidx].Typeuser,
                                foundDocument.family[beneficiaryidx].Kinship,
                                Authorization);

                            //grava no arquivo de eventos
                            _ = _eventsRequestService.CreateEventAsync(BenefityToEventConverter.Parse(foundDocument, new BlockEvent
                            {
                                BlockDate = benefitinfos[i].blockdate,
                                BlockReason = benefitinfos[i].BlockReason,
                                providerGuid = benefitinfos[i].providerguid,
                                providerProductCode = benefitinfos[i].productcode,
                                personGuid = model.family[0].personguid,
                                personName = foundPerson.Name
                            }), Authorization);
                        }
                    }

                    return benefitinfos;
                }

                var updtFamilyobj = await _familyRepository.UpSert((FamilyDb)foundDocument);

                return Ok(foundDocument);

            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpPost("Customer/{token}/Transfer")]
        public async Task<IActionResult> PostTransferPlanAsync(Guid token, [FromBody] QueueModel model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            else
            {
                model.Hubguid = token;
                model.Aggregator = aggregator;

                var valid = await _transferenceService.ValidateAsync(model, Authorization);
                if (valid.Success && string.IsNullOrEmpty(valid.MessageCode))
                {
                    var postQueue = await _queueRequest.Post(model, Authorization);

                    if (postQueue.Success)
                    {
                        //grava nos eventos
                        _ = _eventsRequestService.CreateEventAsync(
                            BenefityToEventConverter.Parse(new FamilyFull
                            {
                                aggregator = model.Aggregator,
                                hubguid = model.Hubguid
                            }, new TransferenceEvent
                            {
                                BlockDate = DateTime.Now,
                                currentProductCode = model.Beneficiary[0].BenefitInfos.productcode,
                                currentProduct = model.Beneficiary[0].BenefitInfos.product,
                                newProductCode = model.Beneficiary[0].BenefitInfos.Transference.ProviderProductCode,
                                newProduct = model.Beneficiary[0].BenefitInfos.Transference.Product,
                                personGuid = model.Beneficiary[0].personguid,
                                providerGuid = model.ProviderGuid,
                                StartDate = DateTime.Now,
                                personName = model.Beneficiary[0].Name,
                                providerName = model.ProviderName,
                                sync = true
                            }
                        ), Authorization);
                        return Ok();
                    }
                    else
                    {
                        return Problem(postQueue.Message);
                    }
                }
                else
                {
                    if (valid.Success)
                    {
                        if (string.IsNullOrEmpty(valid.Message))
                            valid.Message = "Os dados foram salvos com sucesso!";
                        return Ok(valid);
                    }
                    else
                        return StatusCode(valid.HttpStatusCode, valid.Message);
                }
            }
        }

        [HttpPost("Customer/{token}/ValidateBenefit")]
        public async Task<IActionResult> PostValidateBenefitAsync(Guid token, string valid, [FromBody] ValidateBenefitModel model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }

            MethodFeedback mf = new MethodFeedback();
            mf.Success = true;

            //valida pelo configurador de regras
            var validaRulesConfig = await _rulesConfigurationService.Validate(token, aggregator, MovimentTypeEnum.INCLUSÃO, model.Person, model.Benefitinfo, model.EmployeeInfo, model.kinship, Authorization);
            if (validaRulesConfig.Success && valid == "*")
            {
                //valida pelo mandatoty rules
                var validaMandatoryRules = await new MandatoryRulesAddService(_mandatoryRulesRequestService, _taskPanelRequest, _employeeRequest, _mandatoryRulesRuleValidationService)
                    .Validate(token, aggregator, model.Benefitinfo, model.Person,model.kinship, model.typeuser, model.EmployeeInfo.personguid, Authorization, false);
                if (!validaMandatoryRules.Success)
                {
                    mf.Message = "Para concluir a solicitação algumas tarefas devem ser solucionadas no painel de tarefas:";
                    mf.obj = validaMandatoryRules.obj;
                    mf.Success = true;
                    mf.MessageCode = "MANDATORY_RULES_PROBLEM";
                }
            }
            else if (validaRulesConfig.Success)
            {
                mf = validaRulesConfig;
            }
            else
            {
                mf.Message = $"O beneficiário {model.Person.Name} não atende as regras especificadas no contrato {model.Benefitinfo.contractnumber} - {validaRulesConfig.Message},e por isso não podemos concluir a sua solicitação.";
                mf.obj = validaRulesConfig.obj;
                mf.Success = false;
                mf.MessageCode = "RULES_CONFIGURATION_ALERT";
            }

            return Ok(mf);
        }
        [HttpPost]
        [Route("Customer/{token}/GetFamilyValidation")]
        public async Task<object> PostAsyncCPF(Guid token, [FromBody] RulesConfigurationModel RC)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
            try
            {
                if (RC.hubguid != token)
                    return BadRequest("HubGuid do documento inválido");
                if (RC.aggregator != aggregator)
                    return BadRequest("Aggregator do documento inválido");

                return Ok(await _beneficiaryService.ValidateFamiliesByContract(RC, Authorization));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
    }
}