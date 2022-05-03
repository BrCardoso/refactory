using Microsoft.AspNetCore.Mvc;
using System;
using Couchbase.N1QL;
using NetCoreJobsMicroservice.Models;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Commons;
using System.Threading.Tasks;
using NetCoreJobsMicroservice.Repository.Interface;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using NotifierAppService.Models;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class TasksController : Controller
    {
        private readonly ITaskPanelRepository _taskPanelRepository;
        private readonly IBeneficiaryRequestService _beneficiaryRequestService;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public TasksController(ITaskPanelRepository taskPanelRepository, IBeneficiaryRequestService beneficiaryRequestService, IBucketProvider bucket, IConfiguration config)
        {
            _taskPanelRepository = taskPanelRepository;
            _beneficiaryRequestService = beneficiaryRequestService;
            _bucket = bucket.GetBucket("DataBucket001");
            _config = config;
        }

        // GET: api/<controller>
        [HttpGet]
        [Route("Customer/{token}")]
        public async Task<object> GetAsync(string token)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var query = new QueryRequest(@"SELECT g.* FROM DataBucket001 g where g.docType = 'Task' and g.hubguid = $hubguid and g.aggregator = $aggregator")
                    .AddNamedParameter("$hubguid", token)
                    .AddNamedParameter("$aggregator", aggregator);
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<TaskPanel>(query);
                if (result.Success)
                {
                    return Ok(result.Rows);
                }
                else
                {
                    return Problem(result.Message);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }

        }

        // GET api/<controller>/5
        [HttpGet]
        [Route("Customer/{token}/Id/{id}")]
        public async Task<object> GetAsync(Guid id)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            try
            {
                var N1QL = @"SELECT g.* FROM DataBucket001 g where g.docType = 'Task' and g.guid = '{0}';";
                N1QL = string.Format(N1QL, id);
                var query = new QueryRequest(N1QL);
                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<TaskPanel>(query);
                if (result.Success)
                {
                    return Ok(result.Rows);
                }
                else
                {
                    return Problem(result.Message);
                }
                //var result = _bucket.Get<Beneficiary>(id.ToString());
                //return Ok(result.Value);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        // POST api/<controller>
        [HttpPost]
        [Route("Customer/{token}")]
        public async Task<object> PostAsync(Guid token, [FromBody] TaskPanel model, bool? notify)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            model.hubguid = token;
            model.aggregator = aggregator;

            if (ModelState.IsValid)
            {
                try
                {
                    if (model != null)
                    {

                        TaskPanel ret = null;
                        if (model.origin.ToUpper() != "BATEFATURA")
                            ret = _taskPanelRepository.findTask(token, model.guid, model.personguid, model.movtype);


                        if (ret == null)
                        {
                            var a = await _taskPanelRepository.AddTaskPanelAsync(model);
                            if (a)
                            {
                                if (notify == true)
                                {
                                    //envia notificação pra aplicação exibir no front
                                    var modelNotifier = new Notifier()
                                    {
                                        hubguid = model.hubguid.ToString(),
                                        aggregator = model.aggregator,
                                        type = "Painel de tarefas",
                                        title = "Você tem uma nova tarefa no seu painel.",
                                        description = $"Você tem uma nova pendencia para solucionar no painel de tarefas."
                                    };
                                    var cReturn = await operations.PostNotifierAsync(modelNotifier, _config);
                                }
                                return Ok(model);
                            }
                            else
                            {
                                return Problem();
                            }
                        }
                        else
                        {
                            if (model.status.ToUpper() == "ABERTA")
                                return Ok("Ja existe uma solicitação de regularização para o usuário.");
                            else if (model.status.ToUpper() == "FINALIZADO")
                            {
                                var result = await _beneficiaryRequestService.PostFamilyAsync((TaskReturnModel)model, Authorization);
                                if (result.Success)
                                {
                                    var updStatus = _bucket.MutateIn<dynamic>(ret.guid.ToString()).Upsert("status", model.status).Execute();
                                    return Ok(result);
                                }
                                else
                                {
                                    return Problem(result.Message);
                                }
                            }
                            else
                            {
                                var result = _bucket.MutateIn<dynamic>(ret.guid.ToString()).Upsert("status", model.status).Execute();
                                return Ok("Status atualizado.");
                            }
                        }
                    }
                    else
                    {
                        return Conflict("Request body not well formated");
                    }
                }
                catch (Exception ex)
                {
                    return Ok(ex.ToString());
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