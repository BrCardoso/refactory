using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons.Models;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest
{
    public class MandatoryRulesRequestService : IMandatoryRulesRequestService
    {
        IConfiguration config;
        public MandatoryRulesRequestService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<List<MandatoryRules>> getRules(Guid provider)
        {
            try
            {
                using var httpClient = new HttpClient();
                var endpoint = config.GetValue<string>("Endpoints:MandatoryRules");
                using var response = await httpClient.GetAsync(string.Format(endpoint + "/Provider/{0}", provider.ToString()));
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    //Pegando os dados do Rest e armazenando na variável usuários
                    var rulesList = JsonConvert.DeserializeObject<List<MandatoryRules>>(apiResponse);
                    if (rulesList.Count > 0)
                        return rulesList;
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                return null;
            }
            return null;
        }
    }
}