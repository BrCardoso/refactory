using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HuBdoRH.NIT.Repository.Interface;
using Commons.Base;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Couchbase.N1QL;
using Newtonsoft.Json;
using HuBdoRH.NIT.Repository;
using Commons;
using HuBdoRH.NIT.Services;
using HuBdoRH.NIT.Services.Interface;
using Commons.Enums;
using System.Text.Json;
using Newtonsoft.Json.Serialization;

namespace HuBdoRH.NIT.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class NitController : ControllerBase
    {
        private readonly ILogger<NitController> _logger;
        private readonly IConfiguration _config;
        private readonly INitRepository _nitRepository;
        private readonly IChargeService _chargeService;

        public NitController(ILogger<NitController> logger, IConfiguration config, INitRepository nitRepository, IChargeService chargeService)
        {
            _logger = logger;
            _config = config;
            _nitRepository = nitRepository;
            _chargeService = chargeService;
        }

        [HttpGet]
        [Route("GetList")]
        public async Task<object> GetNitTasks()
        {
            IQueryResult<Nit.NitModel> result;

            try
            {
                result = await _nitRepository.GetTasksAsync();

                if (result.Success)
                {
                    var serializerSettings = new JsonSerializerSettings();
                    serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    return Ok(JsonConvert.SerializeObject(result.Rows.OrderByDescending(p => p.createdDate), serializerSettings));
                }
                else
                {
                    return NotFound();
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpPost]
        [Route("CreateNitTaskList")]
        public async Task<ActionResult> PostCreateNitListAsync([FromBody] List<Nit.NitModel> nitTasks)
        {        
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonConvert.SerializeObject(ModelState.Values.SelectMany(v => v.Errors)));
            }

            if (nitTasks.Count == 0)
            {
                return BadRequest("Nenhum item informado");
            }

            if (nitTasks.FirstOrDefault(p => !string.IsNullOrEmpty(p.guid.ToString())) != null)
            {
                return BadRequest("Para inclusao, o atributo 'guid' de todos os itens deve ser vazio.");
            }

            try
            {
                if (await _nitRepository.InsertNitTaskListAsync(nitTasks))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Ocorreu um erro na gravação, tente novamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());
            }
        }


        [HttpPost]
        [Route("CreateNitTask")]
        public async Task<ActionResult> PostCreateNitAsync([FromBody] Nit.NitModel nitTask)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(JsonConvert.SerializeObject(ModelState.Values.SelectMany(v => v.Errors)));
            }

            if (nitTask.guid != Guid.Empty)
            {
                return BadRequest("Para inclusao, o atributo 'guid' deve ser vazio.");
            }

            try
            {
                //Add datetime
                nitTask.createdDate = DateTime.Now;

                if (await _nitRepository.InsertNitTaskAsync(nitTask))
                {
                    return Ok();
                }
                else
                {
                    return BadRequest("Ocorreu um erro na gravação, tente novamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());
            }
        }


        [HttpPost]
        [Route("UpdateNitTask")]
        public async Task<ActionResult> PostUpdateNitAsync([FromBody] Nit.NitModel nitTask)
        {
            string aggregator = Request.GetHeader("aggregator");
            string Authorization = Request.GetHeader("Authorization");
            string refresh_token = Request.GetHeader("refresh_token");

            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);

            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            if (!ModelState.IsValid)
            {
                return BadRequest(JsonConvert.SerializeObject(ModelState.Values.SelectMany(v => v.Errors)));
            }

            if (nitTask.guid == Guid.Empty)
            {
                return BadRequest("Para alteração, o atributo 'guid' deve ser preenchido.");
            }

            try
            {
                if (await _nitRepository.UpdateNitTaskAsync(nitTask))
                {
                    if (nitTask.movementtype == MovimentTypeEnum.PEDIDO_CARGA)
                    {
                        var ret = await _chargeService.PostUpdateCharge(nitTask.movementguid, nitTask, aggregator, Authorization);
                        if (ret)
                        {
                            return Ok();
                        }
                        else
                        {
                            return BadRequest();
                        }
                    }
                    else
                    {
                        return Ok();
                    }                    
                }
                else
                {
                    return BadRequest("Ocorreu um erro na gravação, tente novamente.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());
            }
        }
    }
}
