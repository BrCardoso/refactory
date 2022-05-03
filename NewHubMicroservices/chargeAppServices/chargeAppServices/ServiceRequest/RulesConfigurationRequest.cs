using chargeAppServices.Models;
using ChargeAppServices.ServiceRequest.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest
{
    public class RulesConfigurationRequest : IRulesConfigurationRequest
    {
        private readonly IConfiguration config;
        public RulesConfigurationRequest(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<List<rulesConfigurationModel>> GetAsync(string segcode, string authorization)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", "0001");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            using (var response = await httpClient.GetAsync($"{config.GetValue<string>("Endpoints:RulesConfiguration")}/Segcode/{segcode}"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<rulesConfigurationModel> rc = JsonConvert.DeserializeObject<List<rulesConfigurationModel>>(apiResponse);
                    return rc;
                }
                return null;
            }
        }
    }
}
