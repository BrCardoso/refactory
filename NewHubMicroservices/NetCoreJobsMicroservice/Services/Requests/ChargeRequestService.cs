using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Requests
{
    public class ChargeRequestService : IChargeRequestService
    {
        private readonly IConfiguration _config;
        public ChargeRequestService(IConfiguration config)
        {
            _config = config;
        }

        public async Task<ChargeOrder> getAsync(Guid token, Guid id, string aggregator, string authorization)
        {
            string Uri = string.Format(_config.GetValue<string>("Endpoints:Charge") + "/Customer/{0}/Id/{1}", token, id);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                if (response.IsSuccessStatusCode)
                {
                    var chargeOrder = JsonConvert.DeserializeObject<List<ChargeOrder>>(apiResponse);
                    return chargeOrder.SingleOrDefault();
                }
            }
            return null;
        }

        public async Task<bool> UpdateAsync(List<ChargeOrder> chargeOrder, string authorization)
        {
            string Uri = $"{_config.GetValue<string>("Endpoints:Charge")}/upcharge";
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", chargeOrder[0].Aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(chargeOrder);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}
