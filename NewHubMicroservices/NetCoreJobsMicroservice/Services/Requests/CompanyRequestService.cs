using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Requests
{
    public class CompanyRequestService : ICompanyRequestService
    {
        private readonly IConfiguration config;
        public CompanyRequestService(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<CompanyDB> getCompanyAsync(string cnpj)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Company") + "/CompanyId/{0}", Commons.Helpers.RemoveNaoNumericos(cnpj));
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();


                Uri = string.Format(config.GetValue<string>("Endpoints:Company") + "/{0}", apiResponse);
                using var httpClient1 = new HttpClient();
                using (var response1 = await httpClient1.GetAsync(Uri))
                {
                    string apiResponse1 = await response1.Content.ReadAsStringAsync();
                    if (response.IsSuccessStatusCode)
                    {
                        var company = JsonConvert.DeserializeObject<List<CompanyDB>>(apiResponse1);
                        return company.SingleOrDefault();
                    }

                }
            }
            return null;
        }
    }
}
