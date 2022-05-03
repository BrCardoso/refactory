using Commons;
using Commons.Base;
using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Requests
{
    public class BeneficiaryRequestService : IBeneficiaryRequestService
    {
        private readonly IConfiguration config;
        public BeneficiaryRequestService(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<Person> getPersonAsync(Guid guid)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Person") + "/{0}", guid);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var person = JsonConvert.DeserializeObject<Person>(apiResponse);
                    return person;
                }
            }
            return null;
        }

        public async Task<MethodFeedback> PostFamilyAsync(TaskReturnModel itemArq, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Family") + "/Customer/{0}/TaskResult", itemArq.hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent.ToLower(), System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();

            MethodFeedback ret = new MethodFeedback();
            ret.Success = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var ret1 = JsonConvert.DeserializeObject<NITResponse>(apiResponse);
                ret.obj = ret1;
            }
            else
                ret.Message = "Erro ao tentar registrar a solicitação.";

            return ret;
        }
    }
}
