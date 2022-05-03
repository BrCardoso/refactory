using chargeAppServices.Models;
using ChargeAppServices.ServiceRequest.Interface;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ChargeAppServices.ServiceRequest
{
    public class LoginRequest : ILoginRequest
    {
        private readonly IConfiguration _config;
        public LoginRequest(IConfiguration configuration)
        {
            _config = configuration;
        }

        public async Task<Login.ApplicationResponseModel> GetAsync()
        {
            Login.RequestModel loginReq = new Login.RequestModel
            {
                login = _config.GetValue<string>("RequestId"),
                password = _config.GetValue<string>("RequestPass")
            };

            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(_config.GetValue<string>("Endpoints:Login"));
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(loginReq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");

            HttpResponseMessage response = await client.PostAsync(_config.GetValue<string>("Endpoints:Login"), contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<Login.ApplicationResponseModel>(apiResponse);
            }

            return null;
        }
    }
}
