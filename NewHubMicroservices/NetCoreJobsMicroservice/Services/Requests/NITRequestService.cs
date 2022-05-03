using Commons;
using Microsoft.Extensions.Configuration;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using static Commons.Base.Nit;

namespace NetCoreJobsMicroservice.Services.Requests
{
    public class NITRequestService : INITRequestService
    {
        private readonly IConfiguration config;
        public NITRequestService(IConfiguration configuration)
        {
            config = configuration;
        }

        public async Task<MethodFeedback> PostNITAsync(NitModel itemArq)
        {
            string Uri = $"{config.GetValue<string>("Endpoints:NIT")}/CreateNitTask";
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
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
                var ret1 = JsonConvert.DeserializeObject<NITResponse>(apiResponse);
                ret.obj = ret1;
            }
            else
            {
                ret.Message = "Erro ao tentar registrar a solicitação.";
            }

            return ret;
        }
    }
}
