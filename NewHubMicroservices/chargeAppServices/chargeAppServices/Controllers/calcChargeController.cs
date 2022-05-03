using chargeAppServices.Models;
using ChargeAppServices.Business.Interface;
using ChargeAppServices.Repository.Interface;
using ChargeAppServices.ServiceRequest.Interface;
using Commons;
using Commons.Base;
using Commons.Models;
using Commons.Enums;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using NotifierAppService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace chargeAppServices.Controllers
{
    [Route("api/v1/[controller]")] //api/v1/calcCharge
    [ApiController]
    public class calcChargeController : ControllerBase
    {
        private readonly IBucket _bucket;
        private readonly IBucket _buck999;
        private readonly IConfiguration _config;
        private readonly IRulesConfigurationRequest _rulesConfigurationRequest;
        private readonly ICreditChargeRepository _creditChargeRepository;
        private readonly ICreditChargeBusiness _creditChargeBusiness;
        private readonly ILoginRequest _loginRequest;

        public calcChargeController(
            ICreditChargeRepository creditChargeRepository,
            IRulesConfigurationRequest rulesConfigurationRequest,
            ICreditChargeBusiness creditChargeBusiness,
            ILoginRequest loginRequest,
            IBucketProvider bucketProvider, IConfiguration config)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _buck999 = bucketProvider.GetBucket("DataBucket999");
            _config = config;
            _creditChargeRepository = creditChargeRepository;
            _rulesConfigurationRequest = rulesConfigurationRequest;
            _creditChargeBusiness = creditChargeBusiness;
            _loginRequest = loginRequest;
        }

        [HttpGet]
        [Route("listcharge/Customer/{hubCustomer}/type/{typeBenefit}/competence/{competence}")] //api/v1/calcCharge/listcharge/Customer/65734e6c-248f-4243-8732-cde9c52e94a2/type/VR/competence/2020-06
        public async Task<object> listchargeAsync(Guid hubCustomer, string typeBenefit, string competence)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                List<ChargeOrder> result;
                if (typeBenefit.ToUpper() == "TODOS")
                    result = await _creditChargeRepository.GetListAsync(hubCustomer, aggregator, competence);
                else
                    result = await _creditChargeRepository.GetListAsync(hubCustomer, aggregator, competence, typeBenefit);
                if (result != null)
                    return Ok(result);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                //em caso de falha
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}/Id/{id}")] //api/v1/calcCharge/Customer/65734e6c-248f-4243-8732-cde9c52e94a2/Id/15a67950-6766-4347-84be-ff2cba5ac703
        public async Task<object> Get1Async(Guid token, Guid id)
        {

            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _creditChargeRepository.GetAsync(token, aggregator, id);

                if (result != null)
                    return Ok(new List<ChargeOrder> { result });
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("calculatecharge")] //api/v1/calcCharge/calculatecharge
        public async Task<object> calculatorAsync()
        {
            //não validar, chamado pela function
            try
            {
                var loginMaster = await _loginRequest.GetAsync();
                var result = await _rulesConfigurationRequest.GetAsync("FOOD", loginMaster.credentials.id_token);
                if (result != null)
                {
                    var a = await _creditChargeBusiness.CalculaFoodAsync(result, loginMaster.credentials.id_token);
                    return Ok("OK");
                }
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                //em caso de falha
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost]
        [Route("upcharge")] //api/v1/calcCharge/upcharge
        public async Task<object> PostAsync([FromBody] List<ChargeOrder> oCharges)
        {
            string aggregator = Request.GetHeader("aggregator");
            string Authorization = Request.GetHeader("Authorization");
            string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
            {
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            }

            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }
                if (oCharges == null || oCharges.Count < 1)
                {
                    return Conflict("Request body not well formated");
                }
                else
                {
                    var currentCharge = _creditChargeBusiness.Upsert(oCharges);
                    return Ok();
                }
            }
            catch (Exception ex)
            {
                return Problem(JsonConvert.SerializeObject(ex));
            }
        }

        // POST api/<controller>
        /// <summary>
        /// Retorno do NIT para atualização do status do pedido de carga
        /// </summary>
        /// <param name="token"></param>
        /// <param name="model"></param>
        /// <returns></returns>
        [HttpPost]
        [Route("Customer/{token}/Response")] //api/v1/calcCharge/Customer/65734e6c-248f-4243-8732-cde9c52e94a2/Response
        public async Task<object> PostAsync(Guid token, [FromBody] Nit.NitModel model)
        {
            #region init valid

            string aggregator = Request.GetHeader("aggregator");
            string Authorization = Request.GetHeader("Authorization");
            string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            #endregion

            MethodFeedback mf = new MethodFeedback();
            if (ModelState.IsValid)
            {
                try
                {
                    if (model != null)
                    {
                        var queryRequest = new QueryRequest()
                                                .Statement(string.Format(@"SELECT g.* FROM DataBucket001 g WHERE g.docType = 'creditcharge' AND g.guid = $guid;"))
                                                .AddNamedParameter("$guid", model.movementguid)
                                                .Metrics(false);

                        var queue = await _bucket.QueryAsync<ChargeOrder>(queryRequest);
                        if (queue.Success && queue.Rows.Count > 0)
                        {
                            var cReturnNit = "";
                            var cName = "";

                            if (queue.Rows[0].Charges != null)
                            {
                                foreach (var item in queue.Rows[0].Charges)
                                {
                                    var find = model.charge.beneficiary.Where(x => x.personguid == item.Personguid).FirstOrDefault();
                                    if (find.ItemStatus != null)
                                    {
                                        cReturnNit = find.ItemStatus.response;
                                        cName = item.Name;

                                        item.Requestreturn.Returndate = DateTime.Now;

                                        switch (find.ItemStatus.status)
                                        {
                                            case NitStatusTask.Pendente:
                                                item.Requestreturn.Status = HubMovementStatus.PendenteConfirmacaoCliente;
                                                break;
                                            case NitStatusTask.Andamento:
                                                item.Requestreturn.Status = HubMovementStatus.EmProcessamento;
                                                break;
                                            case NitStatusTask.AguardandoProvedor:
                                                item.Requestreturn.Status = HubMovementStatus.EmProcessamento;
                                                break;
                                            case NitStatusTask.Sucesso:
                                                item.Requestreturn.Status = HubMovementStatus.ProcessadoComSucesso;
                                                break;
                                            case NitStatusTask.Erro:
                                                item.Requestreturn.Status = HubMovementStatus.FalhaNoProcessamento;                                                
                                                break;
                                            default:
                                                break;
                                        }

                                        item.Requestreturn.Protocol = find.ItemStatus.response;
                                        item.Requestreturn.Description = find.ItemStatus.response;
                                    }
                                }
                            }
                            var a = _bucket.Upsert(
                                            queue.Rows[0].Guid.ToString(), new
                                            {
                                                queue.Rows[0].Guid,
                                                docType = "creditcharge",
                                                queue.Rows[0].Hubguid,
                                                queue.Rows[0].Providercustomerguid,
                                                queue.Rows[0].Rulesconfigurationguid,
                                                queue.Rows[0].Aggregator,
                                                queue.Rows[0].Competence,
                                                queue.Rows[0].Subsegcode,
                                                queue.Rows[0].Charges
                                            }
                            );
                            if (a.Success)
                            {
                                if (cReturnNit != "" && cName != "")
                                {
                                    var modelNotifier = new Notifier();
                                    modelNotifier.hubguid = queue.Rows[0].Hubguid.ToString();
                                    modelNotifier.aggregator = queue.Rows[0].Aggregator;
                                    modelNotifier.type = "Food";
                                    modelNotifier.title = "Retorno do pedido de carga de " + cName;
                                    modelNotifier.description = "O pedido de carga foi processado no provedor com o seguinte retorno: " + cReturnNit;

                                    var cReturn = await operations.PostNotifierAsync(modelNotifier, _config);
                                }

                                return Ok(model);
                            }
                            else
                            {
                                return Problem(a.Message + " - " + a.Exception?.ToString());
                            }
                        }
                        else
                        {
                            return Conflict("Charge not found.");
                        }
                    }
                    else
                    {
                        return Conflict("Request body not well formated");
                    }
                }
                catch (Exception ex)
                {
                    return Ok(ex.ToString());
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }
        }
    }
}