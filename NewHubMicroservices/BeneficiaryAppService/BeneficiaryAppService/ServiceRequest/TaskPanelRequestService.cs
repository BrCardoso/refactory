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
    public class TaskPanelRequestService : ITaskPanelRequestService
    {
        IConfiguration config;
        public TaskPanelRequestService(IConfiguration configuration)
        {
            config = configuration;
        }
        public async Task<MethodFeedback> Post(TaskPanelModel itemArq, bool notify, string authorization)
        {
            string Uri = $"{config.GetValue<string>("Endpoints:TaskPanel")}/Customer/{itemArq.hubguid}?notify={notify}";
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);            
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");

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
