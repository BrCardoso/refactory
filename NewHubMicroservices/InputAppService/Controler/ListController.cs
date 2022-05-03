using System;
using System.Collections.Generic;
using System.Linq;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using InputAppService.Models;

namespace InputAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class ListController : ControllerBase
    {
        private readonly ILogger<ListController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public ListController(ILogger<ListController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket999");
            _config = config;
        }

        [HttpGet]
        public async System.Threading.Tasks.Task<object> GetAsync()
        {
            try
            {
                var queryRequest = new QueryRequest()
                   .Statement(@"SELECT g.* FROM DataBucket999 g where g.docType = 'Templates';")
                   .Metrics(false);

                var result = await _bucket.QueryAsync<object>(queryRequest);
                if (result.Success)
                {
                    return Ok(JsonConvert.SerializeObject(result.Rows));
                }
                else
                {
                    return Problem(result.Message + "-" + result.Exception?.ToString());
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        /// <summary>
        /// Retorna listas
        /// </summary>
        /// <param name="type">Tipos de listas= documents, kinship, maritalStatus,movements,reason2ndCopy,reasonBlocks</param>
        /// <returns></returns>
        [HttpGet]
        [Route("{type}")]
        public async System.Threading.Tasks.Task<object> GetAsync(string type)
        {
            try
            {
                var queryRequest = new QueryRequest()
                   .Statement(@"SELECT "+type+" as List FROM DataBucket999 g where g.docType = 'Templates';")
                   .Metrics(false);

                var result = await _bucket.QueryAsync<object>(queryRequest);
                if (result.Success)
                {
                    return Ok(JsonConvert.SerializeObject(result.Rows));
                }
                else
                {
                    return Problem(result.Message + "-" + result.Exception?.ToString());
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }
        
    }
}
