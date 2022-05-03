using HuBdoRH.NIT.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace HuBdoRH.NIT.Services
{
    public class ChargeService : IChargeService
    {
        private readonly IConfiguration config;

        public ChargeService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<bool> PostUpdateCharge(Guid chargeGuid, Commons.Base.Nit.NitModel nitTask, string aggregator, string authorization)
        {
            using (var httpClient = new HttpClient()) {
                string Uri = string.Format(config.GetValue<string>("Endpoints:Charge") + "/Customer/{0}/Response", chargeGuid);
                HttpClient client = new HttpClient();

                client.BaseAddress = new Uri(Uri);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("aggregator", aggregator);
                client.DefaultRequestHeaders.Add("Authorization", authorization);
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                var jsonContent = JsonConvert.SerializeObject(nitTask);
                var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                contentString.Headers.ContentType = new
                MediaTypeHeaderValue("application/json");

                using (HttpResponseMessage response = await client.PostAsync(Uri, contentString))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    return (response.IsSuccessStatusCode);
                }
            }
        }

    }
}
