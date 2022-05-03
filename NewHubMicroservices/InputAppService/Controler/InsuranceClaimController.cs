using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using InputAppService.Models;
using Commons;

namespace InputAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class InsuranceClaimController : ControllerBase
    {
        private readonly ILogger<InsuranceClaimController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public InsuranceClaimController(ILogger<InsuranceClaimController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _config = config;
        }

        /// <summary>
        /// Retorna todas as familias de determinada empresa
        /// </summary>
        /// <param name="token">token da empresa</param>
        /// <returns></returns>
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
                if (token == Guid.Empty)
                {
                    return BadRequest("Missing token.");
                }
                var queryRequest = new QueryRequest()
                   .Statement(@"SELECT g.* FROM DataBucket001 g where 
g.docType = 'InsuranceClaim' 
and g.hubguid = $hubguid
and g.aggregator = $aggregator;")
                   .AddNamedParameter("$hubguid", token)
                   .AddNamedParameter("$aggregator", aggregator)
                   .Metrics(false);

                var result = await _bucket.QueryAsync<InsuranceClaimModel>(queryRequest);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                    return Ok(result.Rows);

                    }
                    else
                        return NotFound("Nenhum registro encontrado.");
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

        /// <summary>
        /// Retorna familia especifica de determinada empresa
        /// </summary>
        /// <param name="token">token da empresa</param>
        /// <param name="id">token da familia</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Customer/{token}/Provider/{provguid}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, Guid provguid)
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
                var queryRequest = new QueryRequest()
                   .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'InsuranceClaim' 
and g.hubguid = $hubguid 
and g.providerguid = $provguid 
and g.aggregator = $aggregator;")
                   .AddNamedParameter("$hubguid", token)
                   .AddNamedParameter("$provguid", provguid)
                   .AddNamedParameter("$aggregator", aggregator)
                   .Metrics(false);

                var result = await _bucket.QueryAsync<InsuranceClaimModel>(queryRequest);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                    return Ok(result.Rows);

                    }
                    else
                        return NotFound("Nenhum registro encontrado.");
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

        [HttpPost]
        [Route("Customer/{token}")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody]InsuranceClaimModel model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (model.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
            if (model.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

            if (ModelState.IsValid)
            {
                try
                {
                    if (token == Guid.Empty)
                    {
                        return BadRequest("Missing token.");
                    }
                    if (model != null)
                    {
                        //inclui a data da operação em cada item do sinistro
                        model.Insuranceclaims.ForEach(u => u.CreationDate = DateTime.Now);

                        //pesquisa se ja existe um documento para o mesmo provedor
                        var result = await operations.findInsuranceClaimAsync(model, _bucket);

                        if (result.Success & result.Rows.Count > 0)
                        {
                            model.guid = result.Rows[0].guid;
                            //inclui os registros de sinistro na listagem do documento ja existente
                            var a = _bucket.MutateIn<InsuranceClaimModel>(model.guid.ToString())
                                .Upsert("Insuranceclaims", model.Insuranceclaims, true)
                                .Execute();
                            return Ok(a);
                        }
                        else
                        {                            
                            model.guid = Guid.NewGuid();
                            //insere novo doc no DB
                            var a = _bucket.Upsert(
                                model.guid.ToString(), new
                                {
                                    model.guid,
                                    docType = "InsuranceClaim",
                                    hubguid = token,
                                    model.aggregator,
                                    model.Insuranceclaims
                                });

                            return Ok(a);
                        }                       
                    }
                    else
                    {
                        return Conflict("Request body not well formated");
                    }
                }
                catch (Exception ex)
                {
                    return Problem(JsonConvert.SerializeObject(ex));
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