using BeneficiaryAppService.Models.External;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest
{
    public class RulesConfigurationRequestService : IRulesConfigurationRequestService
    {
        IConfiguration config;
        public RulesConfigurationRequestService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<RulesConfigurationModel> Get(Guid hubguid, string aggregator, Guid providerguid, string contractNumber, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:RulesConfig") + "/Customer/{0}/Provider/{1}/contract/{2}", hubguid, providerguid, contractNumber);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<List<RulesConfigurationModel>>(apiResponse);
                    return ret[0];
                }
                return null;
            }
        }

    }
}
