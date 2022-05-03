using BeneficiaryAppService.Models;
using BeneficiaryAppService.ServiceRequest.Interfaces;
using Commons;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest
{
    public class QueueRequestService : IQueueRequestService
    {
        IConfiguration config;
        public QueueRequestService(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<MethodFeedback> Post(QueueModel itemArq, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Queue") + "/Customer/{0}", itemArq.Hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.Aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();

            MethodFeedback ret = new MethodFeedback();
            ret.Success = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var ret1 = JsonConvert.DeserializeObject<QueueModel>(apiResponse);
                ret.Message = ret1.guid.ToString();
            }
            else
                ret.Message = "Erro ao tentar registrar a solicitação.";

            return ret;
        }
    }
}
