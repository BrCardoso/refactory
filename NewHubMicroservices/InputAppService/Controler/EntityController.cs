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
using Commons.Base;
using Commons;
using InputAppService.Repository.Interface;
using System.Threading.Tasks;

namespace InputAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class EntityController : ControllerBase
    {
        private readonly IEntityRepository _entityRepository;
        private readonly IConfiguration _config;
        private readonly IBucket _bucket;
        public EntityController(IEntityRepository entityRepository, IConfiguration config, IBucketProvider bucketProvider)
        {
            _entityRepository = entityRepository;
            _config = config;
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        [HttpGet]
        [Route("Customer/{token}")]
        public async Task<object> GetAsync(Guid token)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
            try
            {
                var result = await _entityRepository.GetAsync(token, aggregator);
                if (result != null)
                    return Ok(result);
                else
                    return NotFound("Nenhum registro encontrado.");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }


        [HttpGet]
        [Route("Customer/{token}/type/{type}")]
        public async Task<object> GetAsync(Guid token, string type)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
            try
            {

                var result = await _entityRepository.GetAsync(token, aggregator);
                if (result != null)
                {
                    Entity item = result.entities?.Where(x => x.Type.ToUpper() == type.ToUpper()).FirstOrDefault();
                    result.entities.Clear();
                    result.entities.Add(item);

                    return Ok(result);
                }

                else
                    return NotFound("Listagem não localizada");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }


        [HttpGet]
        [Route("Customer/{token}/type")]
        public async Task<object> GetTypesAsync(Guid token)
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
                   .Statement(@"SELECT distinct e.type FROM DataBucket001 g 
unnest g.entities as e
where g.docType = 'Entity' 
and g.hubguid = $hubguid
and g.aggregator = $aggregator;")
                   .AddNamedParameter("$hubguid", token)
                   .AddNamedParameter("$aggregator", aggregator)
                   .Metrics(false);

                var result = await _bucket.QueryAsync<ListItem2>(queryRequest);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        return Ok(result.Rows);
                    }
                    else
                    {
                        return NotFound("Listagem não localizada");
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

        /// <summary>
        /// Retorna familia especifica de determinada empresa
        /// </summary>
        /// <param name="token">token da empresa</param>
        /// <param name="id">token da familia</param>
        /// <returns></returns>
        [HttpGet]
        [Route("Customer/{token}/Provider/{provguid}")]
        public async Task<object> GetAsync(Guid token, Guid provguid)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var result = await _entityRepository.GetAsync(token, aggregator, provguid);
                if (result != null)
                    return Ok(result);

                else
                    return NotFound("Nenhum registro encontrado.");

            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpPost]
        [Route("Customer/{token}")]
        public async Task<object> PostAsync(Guid token, [FromBody] EntityModel model)
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
                        //pesquisa se ja existe um documento para o mesmo provedor
                        var result = await _entityRepository.GetAsync(model.hubguid, model.aggregator, model.providerguid);
                        if (result != null)
                        {
                            //inclui os registros de sinistro na listagem do documento ja existente
                            model.guid = result.guid;
                            return Ok(_entityRepository.Mutate(model.guid, model.entities));
                        }
                        else
                            return Ok(_entityRepository.Upsert(model));
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
