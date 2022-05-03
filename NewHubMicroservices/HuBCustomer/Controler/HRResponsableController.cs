using System;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Commons.Base;
using static Commons.Helpers;
using System.Threading.Tasks;
using Commons;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Couchbase.Core;
using System.Linq;

namespace HuBCustomerAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HRResponsableController : ControllerBase
    {
        private readonly ILogger<HRResponsableController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public HRResponsableController(ILogger<HRResponsableController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _config = config;
        }

        [HttpPost("Customer/{token}")]
        public async Task<IActionResult> CreateAsync(Guid token,[FromBody] HRResponsableModel.opResponsable newResponsable)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (newResponsable.hubguid != token) return BadRequest("Hubguid diferente da empresa logada");
            if (newResponsable.aggregator != aggregator) return BadRequest("Aggregator diferente da empresa logada");

            if (ModelState.IsValid)
            {
                newResponsable.guid = Guid.NewGuid();
                newResponsable.sessions = EnumerableStringToUpperCaseConverter.Parse(newResponsable.sessions);

                if (newResponsable.sessions.Count == 0)
                    return BadRequest("Voce precisa informar pelo menos uma sessao");

                if (!ValidaCPF(newResponsable.cpf))
                    return BadRequest("O CPF informado nao é valido");

                if (await operations.FindByCPFAsync(newResponsable.cpf, _bucket) is HRResponsableModel.Responsable)
                    return Conflict("Ja existe um responsavel com esse CPF");

                if (!(await operations.UpsertHRResponsableAsync(newResponsable, _bucket)).Success)
                    return BadRequest("Nao foi possivel adicionar o responsavel, tente novamente!");

                return Ok();
            }

            return BadRequest();
        }

        [HttpGet("all/session")]
        public async Task<object> GetAllBySessionAsync([NotEmpty] string aggregator, [NotEmpty] Guid? hubguid, [NotEmpty] string session)
        {
            string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await operations.FindAllBySessionAsync(aggregator, hubguid.Value, session, _bucket));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpGet("all")]
        public async Task<object> GetAllAsync([NotEmpty] string aggregator, [NotEmpty] Guid? hubguid)
        {
            string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                if (ModelState.IsValid)
                {
                    return Ok(await operations.FindAllAsync(aggregator, hubguid.Value, _bucket));
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }
            }
            catch (System.Exception ex)
            {
                return BadRequest(ex.ToString());
            }
        }

        [HttpPut]
        public async Task<object> UpdateAsync([FromBody] HRResponsableModel.opResponsable newResponsableValues)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (ModelState.IsValid)
            {
                newResponsableValues.sessions = EnumerableStringToUpperCaseConverter.Parse(newResponsableValues.sessions);

                if (newResponsableValues.sessions.Count == 0)
                    return BadRequest("Voce precisa informar pelo menos uma sessao");

                if (!(await operations.FindByGuidAsync(newResponsableValues.guid.Value, _bucket) is HRResponsableModel.Responsable))
                    return NotFound("Nao existe nenhum responsavel com esse 'guid' no banco");

                if (!Helpers.ValidaCPF(newResponsableValues.cpf))
                    return BadRequest("O CPF informado nao é valido");

                if (await operations.FindByCPFAsync(newResponsableValues.cpf, _bucket) is HRResponsableModel.Responsable currentUserWithCPF)
                    if (currentUserWithCPF.guid != newResponsableValues.guid)
                        return Conflict("Ja existe um responsavel com esse CPF");

                if (!(await operations.UpsertHRResponsableAsync(newResponsableValues, _bucket) is HRResponsableModel.Responsable updatedResponsable))
                    return BadRequest("Nao foi possivel atualizar as informaçoes do responsavel, tente novamente!");

                return Ok(updatedResponsable);
            }

            return BadRequest();
        }
    }
}
