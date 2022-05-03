using System;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using InputAppService.Models;
using Commons;
using System.Threading.Tasks;
using InputAppService.Repository.Interface;

namespace InputAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class CopartController : ControllerBase
    {
        private readonly ICopartRepository _copartRepository;
        private readonly IConfiguration _config;
        public CopartController(ICopartRepository copartRepository, IConfiguration config)
        {
            _copartRepository = copartRepository;
            _config = config;
        }

        /// <summary>
        /// Retorna todas as familias de determinada empresa
        /// </summary>
        /// <param name="token">token da empresa</param>
        /// <returns></returns>
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
                var a = await _copartRepository.Get(token, aggregator);
                if (a == null)
                    return NotFound("Nenhum registro encontrado.");
                else
                    return Ok(a);
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
                var a = await _copartRepository.Get(token, aggregator, provguid);
                if (a == null)
                    return NotFound("Nenhum registro encontrado.");
                else
                    return Ok(a);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpPost]
        [Route("Customer/{token}")]
        public async Task<object> PostAsync(Guid token, [FromBody] CopartModel model)
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
                    //inclui a data da operação em cada item do sinistro
                    model.Coparticipations.ForEach(u => u.CreationDate = DateTime.Now);

                    //pesquisa se ja existe um documento para o mesmo provedor
                    var result = await _copartRepository.Get(model.hubguid, model.aggregator, model.providerguid);

                    if (result != null)
                        return Ok(_copartRepository.Mutate(result.guid, model.Coparticipations));

                    else
                        return Ok(_copartRepository.Upsert(model));
                }
                catch (Exception ex)
                {
                    return BadRequest(JsonConvert.SerializeObject(ex));
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
