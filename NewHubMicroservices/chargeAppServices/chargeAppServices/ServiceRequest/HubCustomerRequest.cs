using chargeAppServices.Models;
using ChargeAppServices.ServiceRequest.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest
{
    public class HubCustomerRequest : IHubCustomerRequest
    {
        private readonly IConfiguration config;
        public HubCustomerRequest(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<CustomerFull.HuBCustomerModel> GetAsync(Guid guid)
        {
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync($"{config.GetValue<string>("Endpoints:HubCustomer")}/{guid}"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<CustomerFull.HuBCustomerModel> hc = JsonConvert.DeserializeObject<List<CustomerFull.HuBCustomerModel>>(apiResponse);
                    return hc.SingleOrDefault();
                }
                return null;
            }
        }
    }
}
