using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Couchbase.N1QL;
using NetCoreJobsMicroservice.Models;
using System.Linq;
using Newtonsoft.Json;
using Commons;
using Microsoft.Extensions.Configuration;
using NotifierAppService.Models;
using NetCoreJobsMicroservice.Repository.Interface;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class RulesConfigurationController : Controller
    {
        private readonly ILogger<RulesConfigurationController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;
        private readonly IProviderRequestService _providerRequestService;

        public RulesConfigurationController(ILogger<RulesConfigurationController> logger, IBucketProvider bucketProvider, IConfiguration configuration, IRulesConfigurationRepository rulesConfigurationRepository, IProviderRequestService providerRequestService)
        {
            _logger = logger;
            _config = configuration;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _rulesConfigurationRepository = rulesConfigurationRepository;
            _providerRequestService = providerRequestService;
        }

        // GET: api/<controller>
        [HttpGet]
        [Route("ScheduleDueDateFood")]
        public async System.Threading.Tasks.Task<object> GetScheduleDueDateFoodAsync()
        {
            try
            {
                var query = new QueryRequest(@"SELECT g.* FROM DataBucket001 g where g.docType = 'RulesConfiguration'");
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<RulesConfigurationModel>(query);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        foreach (var item in result.Rows)
                        {
                            if (item.foodrules != null)
                            {
                                if (item.duedate == DateTime.Now.Day.ToString())
                                {
                                    //envia notificação pra aplicação exibir no front
                                    var modelNotifier = new Notifier()
                                    {
                                        hubguid = item.hubguid.ToString(),
                                        aggregator = item.aggregator,
                                        type = "Prazo vencendo",
                                        title = "Prazo para pedido de carga do Food vencendo.",
                                        description = $"Você tem mais 5 dias para realizar o pedido de carga do VR e VA da sua empresa do contrato {item.contractnumber}, não perca o prazo."
                                    };
                                    var cReturn = await Commons.operations.PostNotifierAsync(modelNotifier, _config);

                                }
                            }
                        }
                    }
                }
                else
                {
                    return Problem(result.Message);
                }
                return Ok();
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }

        }

        // GET: api/<controller>
        [HttpGet]
        [Route("SegCode/{segcode}")]
        public async System.Threading.Tasks.Task<object> GetAsync(string segcode)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var query = new QueryRequest(@"SELECT g.* FROM DataBucket001 g WHERE g.docType = 'RulesConfiguration' AND segcode = $segcode;")
                    .AddNamedParameter("$segcode", segcode);
                var result = _bucket.Query<RulesConfigurationModel>(query);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                        return result.Rows;
                    else
                        return NotFound();
                }
                else
                {
                    return Problem(result.Message);
                }
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Customer/{token}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var N1QL = @"SELECT g.* FROM DataBucket001 g where g.docType = 'RulesConfiguration' and g.hubguid = $hubguid and g.aggregator = $aggregator;";
                var query = new QueryRequest(N1QL)
                    .AddNamedParameter("$hubguid", token)
                    .AddNamedParameter("$aggregator", aggregator);
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<RulesConfigurationModel>(query);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                        return result.Rows;
                    else
                        return NotFound();
                }
                else
                {
                    return Problem(result.Message);
                }

            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }

        }

        // GET api/<controller>/5
        [HttpGet()]
        [Route("Customer/{token}/Rule/{ruleguid}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, Guid ruleguid)
        {

            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var N1QL = @"SELECT g.* FROM DataBucket001 g WHERE g.guid = $ruleguid and g.docType = 'RulesConfiguration' and g.hubguid = $hubguid and g.aggregator = $aggregator;";
                var query = new QueryRequest(N1QL)
                    .AddNamedParameter("$ruleguid", ruleguid)
                    .AddNamedParameter("$hubguid", token)
                    .AddNamedParameter("$aggregator", aggregator);
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<RulesConfigurationModel>(query);
                if (result.Success)
                {
                    return Ok(result.Rows);
                }
                else
                {
                    return Problem(result.Message);
                }

            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        // GET api/<controller>/5
        [HttpGet()]
        [Route("Customer/{token}/Provider/{providerguid}")]
        public async System.Threading.Tasks.Task<object> GetProvAsync(Guid token, Guid providerguid)
        {

            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var N1QL = @"select distinct g.description ,g.contractnumber from DataBucket001 g
where g.docType ='RulesConfiguration'
and g.providerguid = $providerguid
and g.hubguid = $hubguid and g.aggregator = $aggregator
order by g.description;";
                var query = new QueryRequest(N1QL)
                    .AddNamedParameter("$providerguid", providerguid)
                    .AddNamedParameter("$hubguid", token)
                    .AddNamedParameter("$aggregator", aggregator);
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<RCSearchByProvider>(query);
                if (result.Success)
                {
                    return Ok(result.Rows);
                }
                else
                {
                    return Problem(result.Message);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet("Customer/{token}/Provider/{provider}/contract/{contract}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, Guid provider, string contract)
        {

            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                if (token == Guid.Empty)
                {
                    return BadRequest("Missing token.");
                }

                var result = await _rulesConfigurationRepository.getRCByContractsAsync(token, provider, contract);
                if (result != null)
                {
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

        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody] RulesConfigurationModel model)
        {

            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (ModelState.IsValid)
            {
                model.hubguid = token;
                try
                {
                    if (model != null)
                    {
                        if (model.guid == Guid.Empty)
                        {
                            if (_rulesConfigurationRepository.findRC(token, model.guid, model.providerguid, model.contractnumber) == null)
                                model.guid = Guid.NewGuid();
                            else
                                return Problem("Contrato ja foi cadastrado anteriormente.");
                        }

                        //TODO: RETIRAR QUANDO AS TABELAS DE PREÇOS ESTIVEREM VINDO DO FRONT
                        if (model.generalproductruleshealth != null)
                            foreach (var prod in model.products)
                            {
                                if (string.IsNullOrEmpty(prod.productpricetablename))
                                {
                                    string priceTableName = await _providerRequestService.findPriceTableAsync(token, model.providerguid, prod.code, aggregator, Authorization);
                                    if (!string.IsNullOrEmpty(priceTableName))
                                        prod.productpricetablename = priceTableName;
                                }
                            }
                        var a = _bucket.Upsert(
                            model.guid.ToString(), new
                            {
                                model.guid,
                                docType = "RulesConfiguration",
                                model.hubguid,
                                model.providerguid,
                                model.aggregator,
                                model.segcode,
                                model.contractIssued,
                                model.contractrulename,
                                model.description,
                                model.duedate,
                                model.contractnumber,
                                model.hrresponsable,
                                model.products,
                                model.healthrules,
                                model.generalproductruleshealth,
                                model.generalproductrulesFood,
                                model.foodrules,
                                model.effectivedate
                            });
                        if (a.Success)
                        {
                            return Ok(model);
                        }
                        else
                        {
                            return Problem(a.Message);
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
