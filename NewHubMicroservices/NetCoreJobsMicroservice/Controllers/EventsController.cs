using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Commons;
using Commons.Models;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

using NetCoreJobsMicroservice.Repository.Interfaces;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Newtonsoft.Json;

using static Commons.Helpers;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class EventsController : Controller
    {
        private readonly IEventsRepository _eventsRepository;
        private readonly IProviderRequestService _providerRequestService;
        private readonly IConfiguration _config;

        public EventsController(IEventsRepository eventsRepository, IProviderRequestService providerRequestService, IConfiguration configuration)
        {
            _eventsRepository = eventsRepository;
            _providerRequestService = providerRequestService;
            _config = configuration;
        }

        [HttpPost("Customer/{token}")]
        public async Task<IActionResult> InsertAsync([NotEmpty] Guid token, [FromBody] Event model)
        {
            try
            {
                string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
                var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
                if (!validateUser.Success)
                    return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
                HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }
                else
                {
                    model.hubguid = token;
                    model.aggregator = aggregator;
                    var foundDocument = await _eventsRepository.FindAsync(token, aggregator);

                    if (!(foundDocument is Event))
                        foundDocument = new Event
                        {
                            guid = Guid.NewGuid(),
                            aggregator = aggregator,
                            hubguid = token,
                            blocks = new List<BlockEvent> { },
                            copy2ndRequest = new List<Copy2ndRequestEvent> { },
                            transferences = new List<TransferenceEvent> { }
                        };
                    ProviderDB prov = null;

                    if (model.transferences?.Count > 0)
                    {
                        if (string.IsNullOrEmpty(model.transferences[0].currentProduct) | string.IsNullOrEmpty(model.transferences[0].newProduct) | string.IsNullOrEmpty(model.transferences[0].providerName))
                        {
                            if (prov == null || prov?.guid != model.transferences[0].providerGuid)
                                prov = await _providerRequestService.getProviderAsync(model.transferences[0].providerGuid);
                            if (prov != null)
                            {
                                if (string.IsNullOrEmpty(model.transferences[0].currentProduct) && !string.IsNullOrEmpty(model.transferences[0].currentProductCode))
                                    model.transferences[0].currentProduct = prov.Products.Where(x => x.Providerproductcode == model.transferences[0].currentProductCode).SingleOrDefault().Description;

                                if (string.IsNullOrEmpty(model.transferences[0].newProduct) && !string.IsNullOrEmpty(model.transferences[0].newProductCode))
                                    model.transferences[0].currentProduct = prov.Products.Where(x => x.Providerproductcode == model.transferences[0].newProductCode).SingleOrDefault().Description;


                                if (string.IsNullOrEmpty(model.transferences[0].providerName) && model.blocks[0].providerGuid != Guid.Empty)
                                    model.transferences[0].providerName = prov.Description;

                            }
                        }
                        foundDocument.transferences.Add(model.transferences[0]);

                    }

                    if (model.copy2ndRequest?.Count > 0)
                    {
                        if (string.IsNullOrEmpty(model.copy2ndRequest[0].Product) | string.IsNullOrEmpty(model.copy2ndRequest[0].providerName))
                        {
                            if (prov == null || prov?.guid != model.copy2ndRequest[0].providerGuid)
                                prov = await _providerRequestService.getProviderAsync(model.copy2ndRequest[0].providerGuid);
                            if (prov != null)
                            {
                                if (string.IsNullOrEmpty(model.copy2ndRequest[0].Product) && !string.IsNullOrEmpty(model.copy2ndRequest[0].providerProductCode))
                                    model.copy2ndRequest[0].Product = prov.Products.Where(x => x.Providerproductcode == model.copy2ndRequest[0].providerProductCode).SingleOrDefault().Description;

                                if (string.IsNullOrEmpty(model.copy2ndRequest[0].providerName) && model.copy2ndRequest[0].providerGuid != Guid.Empty)
                                    model.copy2ndRequest[0].providerName = prov.Description;

                            }
                        }
                        foundDocument.copy2ndRequest.Add(model.copy2ndRequest[0]);

                    }

                    if (model.blocks?.Count > 0)
                    {
                        if (string.IsNullOrEmpty(model.blocks[0].Product) | string.IsNullOrEmpty(model.blocks[0].providerName))
                        {
                            if (prov == null || prov?.guid != model.blocks[0].providerGuid)
                                prov = await _providerRequestService.getProviderAsync(model.blocks[0].providerGuid);
                            if (prov != null)
                            {
                                if (string.IsNullOrEmpty(model.blocks[0].Product) && !string.IsNullOrEmpty(model.blocks[0].providerProductCode))
                                    model.blocks[0].Product = prov.Products.Where(x => x.Providerproductcode == model.blocks[0].providerProductCode).SingleOrDefault().Description;

                                if (string.IsNullOrEmpty(model.blocks[0].providerName) && model.blocks[0].providerGuid != Guid.Empty)
                                    model.blocks[0].providerName = prov.Description;

                            }
                        }
                        foundDocument.blocks.Add(model.blocks[0]);

                    }

                    await _eventsRepository.UpSertAsync(foundDocument);

                    return Ok(foundDocument);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet("Customer/{token}/User/{personguid}/Type/{type}")]
        public async Task<IActionResult> FilterAsync([NotEmpty] Guid token, [NotEmpty] string type, [NotEmpty] Guid personguid)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            var foundDocument = await _eventsRepository.FindAsync(token, aggregator);

            if (!(foundDocument is Event))
                return BadRequest("Não foi encontrado nenhum documento");

            if (type == "transferences")
                return Ok(foundDocument.transferences.Where(x => x.personGuid == personguid));

            if (type == "copy2ndRequest")
                return Ok(foundDocument.copy2ndRequest.Where(x => x.personGuid == personguid));

            if (type == "blocks")
                return Ok(foundDocument.blocks.Where(x => x.personGuid == personguid));

            return BadRequest("Não foi encontrado nenhum objeto com esse tipo. (Tipo válidos: transferences, copy2ndRequest e blocks)");
        }

        [HttpGet("Customer/{token}/Type/{type}")]
        public async Task<IActionResult> FilterAsync([NotEmpty] Guid token, [NotEmpty] string type)
        {
            string aggregator = Request.GetHeader("aggregator"); string Authorization = Request.GetHeader("Authorization"); string refresh_token = Request.GetHeader("refresh_token");
            var validateUser = await Helpers.Valida(Request.HttpContext.Connection.RemoteIpAddress, Authorization, refresh_token, aggregator, _config);
            if (!validateUser.Success)
                return StatusCode(validateUser.HttpStatusCode, validateUser.Message);
            HttpContext.Response.Headers.Add("refresh_token", validateUser.Message);

            var foundDocument = await _eventsRepository.FindAsync(token, aggregator);

            if (!(foundDocument is Event))
                return BadRequest("Não foi encontrado nenhum documento");

            if (type == "transferences")
                return Ok(foundDocument.transferences);

            if (type == "copy2ndRequest")
                return Ok(foundDocument.copy2ndRequest);

            if (type == "blocks")
                return Ok(foundDocument.blocks);

            return BadRequest("Não foi encontrado nenhum objeto com esse tipo. (Tipo válidos: transferences, copy2ndRequest e blocks)");
        }

    }
}
