using LoginAppService.Model;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System.Net.Http;
using System.Threading.Tasks;

namespace LoginAppService
{
    public class operations 
    {

        public static async Task<HubCustomerSearch> getHubCustomerAsync(string cnpj,string aggregator, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:HubCustomer") + "/companyid/{0}", cnpj);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                HubCustomerSearch ret = new HubCustomerSearch();
                ret.Success = response.IsSuccessStatusCode;
                if (response.IsSuccessStatusCode)
                {
                    ret = JsonConvert.DeserializeObject<HubCustomerSearch>(apiResponse);
                    ret.Success = true;
                }

                return ret;
            }
        }
    }
}
