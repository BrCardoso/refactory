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
    public class EmployeesController : ControllerBase
    {
        private readonly ILogger<EmployeesController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public EmployeesController(ILogger<EmployeesController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _config = config;
        }

        [HttpGet]
        [Route("Customer/{token}/Employee/{employeeguid}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, Guid employeeguid)
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

                var result = await operations.getEmployeeByGuidAsync(token, aggregator, employeeguid, _bucket);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                        result.Rows[0].employees.RemoveAll(x => x.personguid != employeeguid);
                        return Ok(result.SingleOrDefault());
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

        [HttpGet]
        [Route("Customer/{token}/Registration/{registration}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, string registration)
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

                var result = await operations.getEmployeeByRegistrationAsync(token, aggregator, registration, _bucket);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                        result.Rows[0].employees.RemoveAll(x => x.Registration != registration);
                        return Ok(result.SingleOrDefault());
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

                var result = await operations.getEmployeeAsync(new EmployeesModel { hubguid = token, aggregator = aggregator }, _bucket);
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
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody]EmployeesModel model)
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
                        var retcompany = await operations.UpsertEmployeeAsync(model, _bucket);
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
