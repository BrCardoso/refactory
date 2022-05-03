using Commons;
using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Requests
{
    public class ProviderRequestService : IProviderRequestService
    {
        private readonly IConfiguration config;
        public ProviderRequestService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<string> findPriceTableAsync(Guid token, Guid providerguid, string code, string aggregator, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:PriceTable") + $"/Customer/{token}/Provider/{providerguid}/Product/{code}");
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<Models.PriceTableCB> pc = JsonConvert.DeserializeObject<List<Models.PriceTableCB>>(apiResponse);
                    if (pc.Count == 1)
                    {
                        if (pc[0].prices.Count == 1)
                        {
                            return pc[0].prices[0].Name;

                        }
                    }
                }
                return null;
            }
        }

        public async Task<ProviderDB> getProviderAsync(Guid guid)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Provider") + "/{0}", guid);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<ProviderDB> prov = JsonConvert.DeserializeObject<List<ProviderDB>>(apiResponse);
                    return prov.SingleOrDefault();
                }
                return null;
            }
        }

        public async Task<ProviderStrucDB> getProviderStrucAsync(Guid hubguid, Guid providerguid, string aggregator, string authorization)
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
                    List<ProviderStrucDB> prov = JsonConvert.DeserializeObject<List<ProviderStrucDB>>(apiResponse);
                    return prov.SingleOrDefault();
                }
                return null;
            }
        }
    }
}
