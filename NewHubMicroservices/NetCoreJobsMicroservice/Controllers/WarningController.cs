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
    public class WarningController : Controller
    {
        private readonly ILogger<WarningController> _logger;
        private readonly IBucket _bucket;
        private readonly IConfiguration _config;

        public WarningController(ILogger<WarningController> logger, IBucketProvider bucketProvider, IConfiguration config)
        {
            _logger = logger;
            _bucket = bucketProvider.GetBucket("DataBucket999");
            _config = config;
        }

        [HttpGet]
        [Route("")]
        public async System.Threading.Tasks.Task<object> GetAsync(string description)
        {
            try
            {
                var query = new QueryRequest()
                    .Statement(@"SELECT Warnings FROM DataBucket999 WHERE docType = 'Warning';")
                    .Metrics(false);

                var result = await _bucket.QueryAsync<Solutions>(query);
                if (result.Success)
                {
                    var ret = result.SingleOrDefault(x => x.Warnings != null);
                    if (!string.IsNullOrEmpty(description))
                    {
                        ret.Warnings.RemoveAll(x => x.Description.ToUpper() != description.ToUpper());
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