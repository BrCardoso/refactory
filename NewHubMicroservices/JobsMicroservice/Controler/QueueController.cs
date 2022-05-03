using System.Collections.Generic;
using Couchbase.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Couchbase.N1QL;
using Couchbase.Extensions.DependencyInjection;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Commons;
using JobsMicroservice.Model;

namespace JobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class QueueController : Controller
    {
        private readonly ILogger<QueueController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public QueueController(ILogger<QueueController> logger, IBucketProvider bucketProvider, IConfiguration configuration)
        {
            _logger = logger;
            _config = configuration;
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        [HttpGet]
        [Route("GetModel")]
        public object GetModel()
        {
            try
            {
                var rcm = new QueueModel
                {
                    linkinformation = new LinkInformation(),
                    beneficiary = new List<QueueBeneficiary> { new QueueBeneficiary{
                    Return = new Return(),
                    holder = new Holder{
                        jobinfo = new EmployeeinfoClean{
                        Employeecomplementaryinfos = new List<Commons.Complementaryinfo>{
                            new Commons.Complementaryinfo()
                        }
                        }
                    },
                    benefitinfos = new BenefitInfos{
                        transference = new Transference()
                    }
                    } },
                    charge = new charge
                    {
                        beneficiary = new List<ChargeBeneficiary> {
                            new ChargeBeneficiary{
                            chargeinfo = new Chargeinfo()
                            }
                        },
                        vraddicionalinfo = new VrAddicionalInfo()
                    }
                };
                return Ok(rcm);
            }
            catch (Exception ex)
            {
                return Ok(ex.ToString());
            }
        }

        // GET: api/<controller>
        [HttpGet]
        [Route("Customer/{token}")]
        public object Get()
        {
            try
            {
                var N1QL = @"SELECT g.* FROM DataBucket001 g where g.docType = 'Queue'";
                var query = new QueryRequest(N1QL);
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

        // GET api/<controller>/5
        [HttpGet]
        [Route("Customer/{token}/Id/{id}")]
        public object Get(Guid id)
        {
            try
            {
                var N1QL = @"SELECT g.* FROM DataBucket001 g where g.docType = 'Queue' and g.guid = '{0}';";
                N1QL = string.Format(N1QL, id);
                var query = new QueryRequest(N1QL);
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
                //var result = _bucket.Get<Beneficiary>(id.ToString());
                //return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpOptions()]
        [Route("Customer/{token}/Charge/{id}")]
        public void Options2()
        {
        }

        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody]QueueModel model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (ModelState.IsValid)
            {

                try
                {
                    if (model != null)
                    {
                        var ret = await operations.HandleFileAsync(model.guid, model,_config, _bucket, Authorization);
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
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                return Problem(JsonConvert.SerializeObject(errors));
            }
        }
        
        // OPTIONS api/<controller>
        [HttpOptions()]
        [Route("Customer/{token}")]
        public void Options()
        {
        }
        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}/Response")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody]ResponseNIT model)
        {
            MethodFeedback mf = new MethodFeedback();
            if (ModelState.IsValid)
            {

                try
                {
                    if (model != null)
                    {var queryRequest = new QueryRequest()
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
                                        item.Return = find;
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
                                        item.Return = find;
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
                return Problem(JsonConvert.SerializeObject(errors));
            }
        }

        // OPTIONS api/<controller>
        [HttpOptions()]
        [Route("Customer/{token}/Response")]
        public void Options1()
        {
        }
    }
}
