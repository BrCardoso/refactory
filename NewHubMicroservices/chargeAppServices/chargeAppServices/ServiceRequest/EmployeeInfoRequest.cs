using chargeAppServices.Models;
using ChargeAppServices.ServiceRequest.Interface;
using Commons.Base;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest
{
    public class EmployeeInfoRequest : IEmployeeInfoRequest
    {
        private readonly IConfiguration config;
        public EmployeeInfoRequest(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<EmployeesModel> Get(Guid hubguid, string aggregator, string authorization)
        {
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");
            using (var response = await httpClient.GetAsync($"{config.GetValue<string>("Endpoints:Employees")}/Customer/{hubguid}"))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    List<EmployeesModel> ei = JsonConvert.DeserializeObject<List<EmployeesModel>>(apiResponse);
                    return ei.SingleOrDefault();
                }
                return null;
            }            
        }
    }
}
