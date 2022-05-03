using chargeAppServices.Models;
using ChargeAppServices.ServiceRequest.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest
{
    public class FamilyRequest : IFamilyRequest
    {
        private readonly IConfiguration config;
        public FamilyRequest(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<List<Family>> Get(Guid hubguid, string aggregator, string authorization)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            using (var response = await httpClient.GetAsync($"{config.GetValue<string>("Endpoints:Family")}/Customer/{hubguid}"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<Family> rc = JsonConvert.DeserializeObject<List<Family>>(apiResponse);
                    return rc;
                }
                return null;
            }
        }
    }
}
