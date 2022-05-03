using BeneficiaryAppService.Models;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest
{
    public class ProviderRequestService : IProviderRequestService
    {

        IConfiguration config;
        public ProviderRequestService(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<ProviderDB> GetProvider(Guid providerguid, string productcode)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Provider") + "/{0}", providerguid);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<List<ProviderDB>>(apiResponse);
                    var p = ret[0].Products.Where(x => x.Providerproductcode.ToUpper() == productcode.ToUpper()).FirstOrDefault();
                    ret[0].Products.Clear(); ret[0].Products.Add(p);
                    return ret[0];
                }
                return null;
            }
        }

        public async Task<ProviderStrucDB> GetProviderStruc(Guid hubguid, string aggregator, Guid providerguid, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:ProviderStruc") + "/Customer/{0}/Provider/{1}", hubguid, providerguid);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var ret = JsonConvert.DeserializeObject<List<ProviderStrucDB>>(apiResponse);
                    return ret[0];
                }
                return null;
            }
        }
    }

}
