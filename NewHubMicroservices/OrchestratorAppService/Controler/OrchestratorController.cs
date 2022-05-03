using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Commons.Base;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using NetCoreJobsMicroservice.Models.Base.Output;
using Commons;

namespace NetCoreJobsMicroservice.Controler
{
    [Route("api/v1/[controller]")]
    public class OrchestratorController : Controller
    {
        private readonly ILogger<OrchestratorController> _logger;
        private readonly IConfiguration _config;
        private readonly IBucket _bucket;

        public OrchestratorController(ILogger<OrchestratorController> logger, IConfiguration config, IBucketProvider bucketProvider)
        {
            _config = config;
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        [HttpGet]
        [Route("GetModel")]
        public object GetModel()
        {
            try
            {
                var ret = new CustomerFull
                {
                    Companies= new List<CompanyStruc> { 
                    new CompanyStruc{ 
                        Addresses=new List<Commons.Address>{new Commons.Address() },
                        Complementaryinfos=new List<Commons.Complementaryinfo>{ new Commons.Complementaryinfo() },
                        Employees= new List<Employee>
                        {
                            new Employee
                            {
                                Addresses=new List<Commons.Address>{new Commons.Address() },
                                complementaryinfos =new List<Commons.Complementaryinfo>{ new Commons.Complementaryinfo() },
                                documents=new List<Commons.Document>{ new Commons.Document() },
                                emailinfos=new List<Commons.Emailinfo>{ new Commons.Emailinfo() },
                                Employeecomplementaryinfos=new List<Commons.Complementaryinfo>{ new Commons.Complementaryinfo() },
                                Family=new List<Beneficiary>{ new Beneficiary{ 
                                    Addresses=new List<Commons.Address>{new Commons.Address() },
                                    Benefitinfos=new List<Benefitinfo>{new Benefitinfo() },
                                    complementaryinfos =new List<Commons.Complementaryinfo>{ new Commons.Complementaryinfo() },
                                    documents=new List<Commons.Document>{ new Commons.Document() },
                                    emailinfos=new List<Commons.Emailinfo>{ new Commons.Emailinfo() },
                                    Phoneinfos=new List<Commons.Phoneinfo>{ new Commons.Phoneinfo() }
                                } },
                                Phoneinfos=new List<Commons.Phoneinfo>{ new Commons.Phoneinfo() }
                            }
                        },
                        Copart=new List<Copart>{new Copart() },
                        Entities=new List<Entity>{new Entity() },
                        Insuranceclaim=new List<Insuranceclaim>{new Insuranceclaim() },
                        Providers=new List<ProviderStruc>{new ProviderStruc
                        {
                            accesscredentials=new Accesscredentials(),
                            emailinfos=new List<Commons.Emailinfo>{ new Commons.Emailinfo()},
                            Products=new List<ProductStruc>{new ProductStruc
                            {
                                Complementaryinfo=new List<Commons.Complementaryinfo>{ new Commons.Complementaryinfo() },
                                PriceTable=new List<PriceTable>{new PriceTable{ 
                                    Range = new List<PriceTable.Ranges>{new PriceTable.Ranges() }
                                } },
                            } }
                        } }
                    } },
                    Hierarchy = new List<Hierarchy>
                    {
                        new Hierarchy
                        {
                            Groups = new List<HierarchyGroup>{new HierarchyGroup
                            {
                                Companies = new List<HierarchyCompany>{ new HierarchyCompany() }
                            } }
                        }
                    }
                };
                return Ok(ret);
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }

        }

        [HttpGet]
        [Route("Customer/{token}/Operation/{guid}")]
        public async Task<object> GetAsync(Guid token, Guid guid)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);
            try
            {
                return Ok(await operations.findLogAsync(token, guid, _bucket));
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }

        }

        [HttpPost]
        [Route("Customer/{token}")]
        public async Task<object> PostAsync(Guid token, [FromBody]CustomerFull model)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            Guid eventGuid = Guid.NewGuid();
            _logger.Log(new LogLevel(), "EventGuid:" + eventGuid + "-" + JsonConvert.SerializeObject(model));
            _ = Task.Run(() =>
            {
                operations.UpsertLog(eventGuid,
                    token,
                    "Aberto",
                    new List<LogList> (),
                    _bucket,
                    _logger);
            });

            if (ModelState.IsValid)
            {
                _ = Task.Run(() =>
                {
                    operations.UpsertLog(eventGuid,
                        token,
                        "Em andamento",
                        new List<LogList> {
                            new LogList { Title = "Objeto válido", Date = DateTime.Now, Message = "Objeto do body válido", Exception = false, Success = true
                            } },
                        _bucket,
                        _logger);
                });
                try
                {
                    //localizar contrato no hub
                    await operations.HandleFileAsync(model, _config, eventGuid, _logger, _bucket, Authorization, aggregator).ConfigureAwait(false);

                    return Ok(eventGuid);
                }
                catch (Exception ex)
                {
                    _ = Task.Run(() =>
                    {
                        operations.UpsertLog(eventGuid,
                            token,
                            "Finalizado com erro",
                            new List<LogList> {
                            new LogList { Title = "ERRO DE SISTEMA", Date = DateTime.Now, Message = ex.ToString(), Exception = true, Success = false
                            } },
                            _bucket,
                            _logger);
                    });
                    return Problem(eventGuid + " - " + ex.ToString());
                }
            }
            else
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors);
                _ = Task.Run(() =>
                {
                    operations.UpsertLog(eventGuid,
                        token,
                        "Finalizado com erro",
                        new List<LogList> {
                            new LogList { Title = "Modelo não passou na validação", Date = DateTime.Now, Message = "Objeto do body inválido", Exception = true, Success = false, obj = errors
                            } },
                        _bucket,
                        _logger);
                });
                return Problem(eventGuid + " - " + JsonConvert.SerializeObject(errors));
            }
        }

        // OPTIONS api/<controller>
        [HttpOptions()]
        [Route("Customer/{token}")]
        public void Options(Guid token)
        {
        }
    }
}