using System;
using System.Collections.Generic;
using Commons;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using ProvidersAppservice.Models;
using ProvidersAppservice.Repository.Interface;

namespace ProvidersAppservice.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ProviderController : ControllerBase
    {
        private readonly IProviderRepository _providerRepository;

        public ProviderController(IProviderRepository providerRepository)
        {
            _providerRepository = providerRepository;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<object> GetAsync()
        {
            try
            {
                var result = await _providerRepository.Get();
                if (result != null)
                {
                    return Ok(result);
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
        [Route("{token}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token)
        {
            try
            {
                var result = await _providerRepository.Get(token);
                if (result != null)
                {
                    return Ok(new List<ProviderCB> { result });
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
        [Route("CompanyId/{companyid}")]
        public async System.Threading.Tasks.Task<object> GetAsync(string companyid)
        {
            try
            {
                var result = await _providerRepository.FindByCNPJ(companyid);
                if (result != null)
                    return Ok(result.guid.ToString());
                else
                    return NotFound("Nenhum registro encontrado.");

            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());

            }
        }

        // POST: api/Person
        [HttpPost]
        public async System.Threading.Tasks.Task<object> PostAsync([FromBody] ProviderCB provider)
        {
            try
            {
                //verifica se objeto está nulo
                if (provider != null)
                {
                    provider = await _providerRepository.Upsert(provider);

                    return Ok(provider);
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
    }
}
