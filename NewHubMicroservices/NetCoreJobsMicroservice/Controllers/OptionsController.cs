using System.Collections.Generic;
using Couchbase.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Couchbase.N1QL;
using NetCoreJobsMicroservice.Models;
using Couchbase.Extensions.DependencyInjection;
using System.Linq;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;

namespace NetCoreJobsMicroservice.Controllers
{
    [Route("api/v1/[controller]")]
    public class OptionsController : Controller
    {
        private readonly ILogger<OptionsController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public OptionsController(ILogger<OptionsController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket999");
            _config = config;
        }

        [HttpGet]
        [Route("")]
        public async System.Threading.Tasks.Task<object> GetAsync(string type)
        {
            try
            {
                var query = new QueryRequest()
                    .Statement(@"SELECT g.list FROM DataBucket999 g WHERE g.docType = 'Options';")
                    .Metrics(false);

                var result = await _bucket.QueryAsync<Options>(query);
                if (result.Success)
                {
                    var ret = result.SingleOrDefault();
                    if (!string.IsNullOrEmpty(type))
                    {
                        ret.list.RemoveAll(x => x.type.ToUpper() != type.ToUpper());
                    }
                    return Ok(ret);
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



    }
}