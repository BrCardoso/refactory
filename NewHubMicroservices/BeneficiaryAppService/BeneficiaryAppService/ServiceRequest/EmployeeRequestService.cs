using BeneficiaryAppService.Models;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest
{
    public class EmployeeRequestService : IEmployeeRequestService
    {
        IConfiguration config;
        public EmployeeRequestService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<EmployeeInfo> getInfo(Guid hubguid, string aggregator, Guid employeeguid, string authorization)
        {
            try
            {
                string Uri = string.Format(config.GetValue<string>("Endpoints:Employees") + "/Customer/{0}", hubguid);

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
                using (var response = await httpClient.GetAsync(Uri))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var ret = JsonConvert.DeserializeObject<List<EmployeeDB>>(apiResponse);
                        var result = ret[0].employees.Where(x => x.personguid == employeeguid).FirstOrDefault();

                        return result;
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                return null;
            }
        }

        public async Task<EmployeeDB> getInfoByPersonGuid(Guid hubguid, string aggregator, Guid employeeguid, string authorization)
        {
            try
            {
                string Uri = string.Format(config.GetValue<string>("Endpoints:Employees") + "/Customer/{0}", hubguid);

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
                using (var response = await httpClient.GetAsync(Uri))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        var ret = JsonConvert.DeserializeObject<List<EmployeeDB>>(apiResponse);
                        ret[0].employees.RemoveAll(x => x.personguid != employeeguid);

                        return ret.SingleOrDefault();
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                return null;
            }
        }

        public async Task<EmployeeDB> getInfoByRegistration(Guid hubguid, string aggregator, string registration, string authorization)
        {
            try
            {
                string Uri = string.Format(config.GetValue<string>("Endpoints:Employees") + $"/Customer/{hubguid}/Registration/{registration}");

                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
                using (var response = await httpClient.GetAsync(Uri))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        return JsonConvert.DeserializeObject<EmployeeDB>(apiResponse);
                    }
                    return null;
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                return null;
            }
        }

        public async Task<EmployeeModel> Post(Guid hubguid, string aggregator, EmployeeInfo employeeInfo, string authorization)
        {
            EmployeeModel employee = new EmployeeModel
            {
                hubguid = hubguid,
                aggregator = aggregator,
                employees = new List<EmployeeInfo>
                {
                    employeeInfo
                }
            };

            string Uri = string.Format(config.GetValue<string>("Endpoints:Employees") + "/Customer/{0}", hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(employee);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Models.EmployeeModel>(apiResponse);
            }

            return null;
        }
    }
}
