using System;
using System.Net.Http;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;

namespace FunctionAppChargeFood
{
    public static class Function1
    {
        [FunctionName("FunctionChargeFood")]
        //public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 0 23 * * *")]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"FunctionChargeFood - iniciando execução em: {DateTime.Now}");

            string Uri = Environment.GetEnvironmentVariable("UrlFoodAPI");
            log.LogInformation($"URI:{Uri} - executado em: {DateTime.Now}");
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                log.LogInformation($"Respose:{apiResponse} - fim da execução em: {DateTime.Now}");
                log.LogInformation($"C# FunctionChargeFood function finalizado em: {DateTime.Now}");

                //return apiResponse;
            }
        }

        [FunctionName("ChargeDueDate")]
        //public static async System.Threading.Tasks.Task RunAsync([TimerTrigger("0 */5 * * * *")]TimerInfo myTimer, ILogger log)
        public static async System.Threading.Tasks.Task ChargeDueDateRunAsync([TimerTrigger("0 0 5 * * *")] TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"ChargeDueDate - iniciando execução em: {DateTime.Now}");

            string Uri = Environment.GetEnvironmentVariable("UrlRuleConfigAPI");
            log.LogInformation($"URI:{Uri} - executado em: {DateTime.Now}");
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                log.LogInformation($"Respose:{apiResponse} - fim da execução em: {DateTime.Now}");
                log.LogInformation($"C# FunctionChargeFood function finalizado em: {DateTime.Now}");

                //return apiResponse;
            }
        }
    }
}
