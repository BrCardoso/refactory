using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using HuBCustomerAppService.Models;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System.Linq;

namespace HuBCustomerAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class HuBCustomerController : ControllerBase
    {
        private readonly ILogger<HuBCustomerController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public HuBCustomerController(ILogger<HuBCustomerController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket001");
            _config = config;
        }

        [HttpGet]
        [Route("{token}")]
        public async System.Threading.Tasks.Task<object> GetAsync(Guid token)
        {
            try
            {
                if (token == Guid.Empty)
                {
                    return BadRequest("Missing token.");
                }

                var result = await operations.getContractAsync(token, _bucket);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                        return Ok(result.Rows);

                    }
                    else
                        return NotFound("Nenhum registro encontrado.");
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

        [HttpGet]
        [Route("companyid/{companyid}")]
        public async System.Threading.Tasks.Task<object> GetAsync(string companyid)
        {
            string aggr = Request.GetHeader("aggregator");
            Models.Response.IdentityVal ret = new Models.Response.IdentityVal();
            try
            {
                var result = await operations.getContractAsync(companyid, _bucket);
                if (result.Success)
                {
                    if (result.Rows.Count > 0)
                    {
                        //code
                        if (string.IsNullOrEmpty(aggr)) { ret.aggregator = "0001"; aggr = "0001"; }
                        else
                        {
                            ret.aggregator = aggr;
                        }
                            

                        var depth = ret.aggregator.Split("-").Length;
                        var lt = result.Rows[0].hierarchy.Groups.Where(x => x.Code == ret.aggregator.ToString()).Select(o => new { Group = o, Company = o.Companies.ToList() }).ToList();
                        if (depth == 3)
                        {
                            //referencia de hierarquia dentro da organização;
                            lt = result.Rows[0].hierarchy.Groups.Where(
                                x => x.Companies.Any(
                                    y => y.Branches.Contains(ret.aggregator.Split("-")[2].ToString())))
                                .Select(o => new { Group = o, Company = o.Companies.Where(d => d.Branches.Contains(ret.aggregator.Split("-")[2].ToString())).ToList() })
                                .ToList();

                        }
                        else if (depth == 2)
                        {
                            lt = result.Rows[0].hierarchy.Groups.Where(
                               x => x.Code == ret.aggregator.Split("-")[0].ToString() && x.Companies.Any(
                                   y => y.Code == ret.aggregator.Split("-")[1].ToString()))
                               .Select(o => new { Group = o, Company = o.Companies.Where(d => d.Code == ret.aggregator.Split("-")[1].ToString()).ToList() })
                               .ToList();
                        }
                        lt[0].Group.Companies.Clear();
                        lt[0].Group.Companies.AddRange(lt[0].Company);

                        ret.aggregator = aggr;
                        ret.groups = lt;
                        ret.companyguid = result.Rows[0].guid;
                        ret.companyid = result.Rows[0].contractIssued;
                        ret.companyName = lt[0].Group.Name;
                        ret.Status = result.Rows[0].status;

                        return Ok(ret);

                    }
                    else
                        return NotFound("Nenhum registro encontrado.");
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

        [HttpPost]
        [Route("{token}")]
        public async System.Threading.Tasks.Task<object> PostAsync(Guid token, [FromBody]HuBCustomerModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    //verifica se objeto está nulo
                    if (model == null)
                        return Conflict("Request body not well formated");
                    else
                    {
                        var retcompany = await operations.UpsertAsync(model, Request.Headers["Aggregator"], _bucket);
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

        [HttpOptions()]
        [Route("{token}")]
        public void Options(Guid token)
        {
        }

    }
}
