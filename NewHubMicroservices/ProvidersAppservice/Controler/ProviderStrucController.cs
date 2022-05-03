using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using Commons;
using ProvidersAppservice.Models;
using System.Linq;
using ProvidersAppservice.Business.Interface;
using ProvidersAppservice.Repository.Interface;
using System.Collections.Generic;

namespace ProvidersAppservice.Controler
{
    [Route("api/v1/[controller]")]
    public class ProviderStrucController : Controller
    {
        private readonly IConfiguration _config;
        private readonly IProviderStrucBusiness _providerStrucBusiness;
        private readonly IProviderStrucRepository _providerStrucRepository;
        public ProviderStrucController(IProviderStrucBusiness providerStrucBusiness, IProviderStrucRepository providerStrucRepository, IConfiguration config)
        {
            _providerStrucBusiness = providerStrucBusiness;
            _providerStrucRepository = providerStrucRepository;
            _config = config;
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
                var result = await _providerStrucRepository.GetAll(token, aggregator);
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
        [Route("Customer/{token}/Id/{id}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token, Guid id)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
            try
            {
                var result = await _providerStrucRepository.Get(id);
                if (result != null)
                {
                    var a = await _providerStrucBusiness.GetProductNames(result);
                    var lt = new List<ProviderCustomerCB>();
                    lt.Add(a);
                    return Ok(lt);
                }
                else
                    return NotFound("Nenhum registro encontrado.");
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
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
                var result = await _providerStrucRepository.GetByProviderGuid(token, aggregator, providerguid);
                if (result != null)
                {
                    var a = await _providerStrucBusiness.GetProductNames(result);
                    var lt = new List<ProviderCustomerCB>();
                    lt.Add(a);
                    return Ok(lt);
                }
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
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody] ProviderStrucCB model)
        {
            string aggregator = Request.GetHeader("aggregator"); 
            string Authorization = Request.GetHeader("Authorization"); 
            string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);

            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (ModelState.IsValid)
            {
                if (model.Hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
                if (model.Aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

                try
                {
                    //verifica se objeto está nulo
                    if (model == null)
                        return Conflict("Request body not well formated");
                    else
                    {
                        var retcompany = await _providerStrucBusiness.UpsertAsync(model);
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