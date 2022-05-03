using BIAppService.Model.Request;
using BIAppService.ServiceRequest.Interface;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace BIAppService.ServiceRequest
{

    public class ServiceRequests : IServiceRequests
	{

        private readonly IConfiguration _config;
        public ServiceRequests(IConfiguration configuration)
        {
            _config = configuration;
        }
        public async Task<InsuranceClaimRequest> GetInsuranceClaimAsync(Guid hubguid, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:InsuranceClaim")}/Customer/{hubguid}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", "0001");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<InsuranceClaimRequest>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values;
                }

                return null;
            }
        }
        public async Task<HubCustomerRequest> GetHubCustomerAsync(Guid hubguid, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:HubCustomer")}/{hubguid}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", "0001");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<List<HubCustomerRequest>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values?[0];
                }

                return null;
            }
        }
        public async Task<List<Family>> GetBeneficiariesAsync(Guid hubguid, string authorization) 
        {
            string uri = $"{_config.GetValue<string>("Endpoints:Family")}/Customer/{hubguid}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", "0001");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<List<Family>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values;
                }

                return null;
            }
        }
        public async Task<Provider> GetProviderAsync(Guid providerguid, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:Provider")}/{providerguid}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", "0001");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<List<Provider>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values?[0];
                }

                return null;
            }
        }

        public async Task<Company> GetCompanyAsync(string companyguid, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:Company")}/Id/{companyguid}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", "0001");
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<List<Company>>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values?[0];
                }

                return null;
            }
        }
    }
}
