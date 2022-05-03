using Microsoft.AspNetCore.Mvc;
using System;
using Couchbase.N1QL;
using NetCoreJobsMicroservice.Repository.Interface;
using NetCoreJobsMicroservice.Models;
using System.Linq;
using Newtonsoft.Json;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")] //api/v1/MandatoryRules
    public class MandatoryRulesController : Controller
    {
        private readonly IMandatoryRulesRepository _mandatoryRulesRepository;

        public MandatoryRulesController(IMandatoryRulesRepository mandatoryRulesRepository)
        {
            _mandatoryRulesRepository = mandatoryRulesRepository;
        }

        [HttpGet]
        public object Get()
        {
            try
            {
                var result = _mandatoryRulesRepository.GetAll();
                if (result != null)
                    return Ok(result);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Provider/{token}")]
        public object Get(Guid token)
        {
            try
            {
                var result = _mandatoryRulesRepository.GetByProvider(token);
                if (result != null)
                    return Ok(result);
                else
                    result = _mandatoryRulesRepository.GetBySegment("SAUDE"); //APENAS SAUDE TEM AS CONFIGURAÇÕES
                
                if (result != null)
                    return Ok(result);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Rule/{token}")]
        public object GetRule(Guid token)
        {
            try
            {
                var result = _mandatoryRulesRepository.Get(token);
                if (result != null)
                    return Ok(result);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet]
        [Route("Provider/{token}/Movimenttype/{type}")]
        public object Get(Guid token, string type)
        {
            try
            {
                var result = _mandatoryRulesRepository.GetByProvider(token, type);
                if (result != null)
                    return Ok(result);
                else
                    return NotFound();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpPost]
        public object Post([FromBody] MandatoryRules rule)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }
                if (_mandatoryRulesRepository.Upsert(rule))
                    return Ok();
                else
                    return Problem();
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
    }
}
