using Commons.Base;
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
    public class EmployeeRequestService : IEmployeeRequestService
    {
        private readonly IConfiguration config;
        public EmployeeRequestService(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<Employeeinfo> getEmployeeInfoAsync(Guid employeeguid, Guid hubguid, string aggregator, string authorization)
        {
            string Uri = $"{ string.Format(config.GetValue<string>("Endpoints:Employees")) }/Customer/{hubguid}/Employee/{employeeguid}";
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var employeeinfos = JsonConvert.DeserializeObject<List<Employeeinfo>>(apiResponse);
                    return employeeinfos.SingleOrDefault();
                }
            }
            return null;
        }
    }
}
