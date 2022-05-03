using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HuBCustomerAppService.Models;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Linq;
using Commons;

namespace HuBCustomerAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class OnBoardingController : ControllerBase
    {
        private readonly ILogger<OnBoardingController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public OnBoardingController(ILogger<OnBoardingController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _config = config;
        }

        [HttpGet]
        [Route("Customer/{token}/aggregator/{aggregator}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, string aggregator)
        {
            string _aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
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

                var result = await operations.getOnBoardingAsync(new onboarding { hubguid = token, aggregator = aggregator }, _bucket);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                        return Ok(result.Rows);

                    }
                    else
                    {
                        return Ok(new List<onboarding>() { 
                            new onboarding { 
                                guid = Guid.NewGuid(),
                                hubguid = token, 
                                aggregator = aggregator, 
                                steps = new steps() 
                            }
                        });
                    }
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

                var result = await operations.getOnBoardingAsync(new onboarding { hubguid = token, aggregator = aggregator }, _bucket);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                        return Ok(result.Rows);

                    }
                    else
                    {
                        return Ok(new List<onboarding>() {
                            new onboarding {
                                guid = Guid.NewGuid(),
                                hubguid = token,
                                aggregator = aggregator,
                                steps = new steps()
                            }
                        });
                    }
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
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody]onboarding model)
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
                    //verifica se objeto está nulo
                    if (model == null)
                        return Conflict("Request body not well formated");
                    else
                    {
                        var retcompany = await operations.UpsertOnBoardingAsync(model, _bucket);
                        return Ok(retcompany);
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
