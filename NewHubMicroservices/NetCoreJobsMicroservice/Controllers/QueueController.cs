using Couchbase.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Couchbase.N1QL;
using NetCoreJobsMicroservice.Models;
using Couchbase.Extensions.DependencyInjection;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Commons;
using NotifierAppService.Models;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using NetCoreJobsMicroservice.Business.Interfaces;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class QueueController : Controller
    {
        private readonly ILogger<QueueController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;
        private readonly IChargeRequestService _charge;
        private readonly IChargeBusiness _chargebusiness;
        private readonly IMovimentQueueBusiness _movimentQueueBusiness;

        public QueueController(ILogger<QueueController> logger, IBucketProvider bucketProvider, IConfiguration configuration, IChargeRequestService charge, IChargeBusiness chargebusiness, IMovimentQueueBusiness movimentQueueBusiness)
        {
            _logger = logger;
            _config = configuration;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _charge = charge;
            _chargebusiness = chargebusiness;
            _movimentQueueBusiness = movimentQueueBusiness;
        }

        // GET api/<controller>/5
        [HttpGet]
        [Route("Customer/{token}/Id/{id}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, Guid id)
        {
            string aggregator = Request.GetHeader("aggregator");
            string Authorization = Request.GetHeader("Authorization");
            string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var query = new QueryRequest(@"SELECT g.* FROM DataBucket001 g where g.docType = 'Movement' and g.hubguid = $hubguid and g.guid = $guid and g.aggregator = $aggregator;")
                    .AddNamedParameter("$hubguid", token)
                    .AddNamedParameter("$guid", id)
                    .AddNamedParameter("$aggregator", aggregator);
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<QueueModel>(query);
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

        [HttpPut]
        [Route("Customer/{token}/Charge/{id}")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, Guid id)
        {
            string aggregator = Request.GetHeader("aggregator"); 
            string Authorization = Request.GetHeader("Authorization"); 
            string refresh_token = Request.GetHeader("refresh_token");
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
                var ret = await _chargebusiness.HandleChargeAsync(token, id, aggregator, Authorization);

                return Ok(ret);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}")]
        public async Task<object> PostAsync(Guid token, [FromBody] QueueModel model)
        {
            string aggregator = Request.GetHeader("aggregator");
            string Authorization = Request.GetHeader("Authorization"); 
            string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return BadRequest(JsonConvert.SerializeObject(errors));
            }

            model.hubguid = token;
            model.aggregator = aggregator;

            try
            {
                if (model != null)
                {
                    if (string.IsNullOrEmpty(model.beneficiary[0].benefitinfos.providerproductCode)) model.beneficiary[0].benefitinfos.providerproductCode = model.beneficiary[0].benefitinfos.productcode;
                    if (string.IsNullOrEmpty(model.linkinformation?.contractnumber)) model.linkinformation = new Commons.Base.Nit.LinkInformation { contractnumber = model.beneficiary[0].benefitinfos.contractnumber };

                    var ret = await _movimentQueueBusiness.HandleFileAsync(model.guid, model, Authorization);
                    if (ret.Success)
                    {
                        return Ok(model);
                    }
                    else
                    {
                        return Problem(ret.Message);
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

        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}/Response")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody] ResponseNIT model)
        {
            MethodFeedback mf = new MethodFeedback();
            if (ModelState.IsValid)
            {
                try
                {
                    if (model != null)
                    {
                        var queryRequest = new QueryRequest()
                    .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Movement' 
and g.guid = $guid;")
                    .AddNamedParameter("$guid", model.movementguid)
                    .Metrics(false);

                        var queue = await _bucket.QueryAsync<QueueModel>(queryRequest);
                        if (queue.Success && queue.Rows.Count > 0)
                        {
                            if (queue.Rows[0].charge != null)
                            {
                                foreach (var item in queue.Rows[0].charge.beneficiary)
                                {
                                    var find = model.Return.Where(x => x.personguid == item.personguid).FirstOrDefault();
                                    if (find != null)
                                    {
                                        find.datetime = DateTime.Now;
                                        item.ItemStatus = new Commons.Base.Nit.StatusItemDetails
                                        {
                                            status = find.status,
                                            response = find.response,
                                            datetime = find.datetime
                                        };
                                    }
                                }
                            }
                            if (queue.Rows[0].beneficiary != null)
                            {
                                foreach (var item in queue.Rows[0].beneficiary)
                                {
                                    var find = model.Return.Where(x => x.personguid == item.personguid).FirstOrDefault();
                                    if (find != null)
                                    {
                                        find.datetime = DateTime.Now;
                                        item.ItemStatus = new Commons.Base.Nit.StatusItemDetails
                                        {
                                            status = find.status,
                                            response = find.response,
                                            datetime = find.datetime
                                        };
                                    }
                                    else
                                    {
                                        return Problem("PersonGuid não localizado na movimentação.");
                                    }
                                }
                            }
                            var a = _bucket.Upsert(
                                 queue.Rows[0].guid.ToString(), new
                                 {                                     
                                     queue.Rows[0].guid,
                                     docType = "Movement",
                                     queue.Rows[0].incdate,
                                     queue.Rows[0].hubguid,
                                     queue.Rows[0].aggregator,
                                     queue.Rows[0].providerguid,
                                     queue.Rows[0].movementtype,
                                     queue.Rows[0].status,
                                     queue.Rows[0].linkinformation,
                                     queue.Rows[0].beneficiary,
                                     queue.Rows[0].charge
                                 });
                            if (a.Success)
                            {
                                var modelNotifier = new Notifier();
                                modelNotifier.hubguid = queue.Rows[0].hubguid.ToString();
                                modelNotifier.aggregator = queue.Rows[0].aggregator;
                                modelNotifier.type = "Movecad";
                                modelNotifier.title = "Movimentação enviada para o provedor";
                                modelNotifier.description = "Foi enviado nova movimentação para o provedor ";

                                var cReturn = await operations.PostNotifierAsync(modelNotifier, _config);

                                return Ok(model);
                            }
                            else
                            {
                                return Problem(a.Message + " - " + a.Exception?.ToString());
                            }
                        }
                        else
                        {
                            return Conflict("Movement not found.");
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