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
    public class HubCustomerRequestService : IHubCustomerRequestService
    {
        private readonly IConfiguration config;
        public HubCustomerRequestService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<HubCustomerOut> getHubCustomerAsync(Guid token)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:HubCustomer") + "/{0}", token);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var customer = JsonConvert.DeserializeObject<List<HubCustomerOut>>(apiResponse);
                    return customer.SingleOrDefault();
                }
            }
            return null;
        }
    }
}
