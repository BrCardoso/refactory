using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Couchbase.Core;
using NetCoreJobsMicroservice.Repository.Interface;

namespace NetCoreJobsMicroservice.Services.Requests
{
    public class ConferenceRequestService : IConferenceRequestService
    {
        private readonly IConfiguration _config;
        private readonly IRulesConfigurationRepository _rulesConfigurationRepository;

        public ConferenceRequestService(IConfiguration configuration, IRulesConfigurationRepository rulesConfigurationRepository)
        {
            _config = configuration;
            _rulesConfigurationRepository = rulesConfigurationRepository;
        }

        public async Task<FamilyHub> GetFamilyByCardNumberAsync(Guid hubguid, string cardNumber, string aggregator, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:Family")}/Customer/{hubguid}/CardNumber/{cardNumber}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<FamilyHub>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values;
                }

                return null;
            }
        }
        public async Task<FamilyHub> GetFamilyByCPFAndBirthAsync(Guid hubguid, string cpf, DateTime birthdate, string aggregator, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:Family")}/Customer/{hubguid}/Document/{cpf}/Birth/{birthdate:yyyy-MM-dd}";

            using var httpClient = new HttpClient();

            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", $"Bearer {authorization}");

            using (HttpResponseMessage response = await httpClient.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<FamilyHub>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                    return values;
                }

                return null;
            }
        }

        public async Task<List<FamilyHub>> GetFemiliesAsync(Guid hubguid, string aggregator, Guid providerguid, IEnumerable<string> contracts, string authorization)
        {
            string uri = $"{_config.GetValue<string>("Endpoints:Family")}/Customer/{hubguid}/Provider/{providerguid}?";

            using var client = new HttpClient();
            contracts.All(x =>
            {
                uri += $"contract={x}&";
                return true;
            });

            client.DefaultRequestHeaders.Add("aggregator", aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);

            using (HttpResponseMessage response = await client.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var apiresponse = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<List<FamilyHub>>(apiresponse, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                }
                return null;
            }
        }

        public async Task<ConferencePriceTableName> GetProductPriceTableAsync(Guid hubguid, Guid providerguid, string contract, IBucket _bucket)
        {
            var result = await _rulesConfigurationRepository.getRCByContractsAsync(hubguid, providerguid, contract);
            if (result != null)
            {
                return Parse(result[0]);
            }
            return null;
        }

        private ConferencePriceTableName Parse(RulesConfigurationModel v)
        {
            ConferencePriceTableName ret = new ConferencePriceTableName
            {
                aggregator = v.aggregator,
                contractnumber = v.contractnumber,
                guid = v.guid,
                hubguid = v.hubguid,
                products = new List<ConferencePriceTableName.Product>(),
                providerguid = v.providerguid
            };
            foreach (var prod in v.products)
            {
                ret.products.Add(new ConferencePriceTableName.Product
                {
                    code = prod.code,
                    productpricetablename = prod.productpricetablename
                });
            }
            return ret;
        }

        public async Task<ConferenceEmploees> GetSalariesAsync(Guid hubguid, string aggregator, string authorization)
        {

            string uri = $"{_config.GetValue<string>("Endpoints:Employees")}/Customer/{hubguid}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("aggregator", aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            using (HttpResponseMessage response = await client.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    var result = JsonSerializer.Deserialize<List<ConferenceEmploees>>(await response.Content.ReadAsStringAsync());
                    return result.SingleOrDefault();
                }

                return null;
            }
        }

        public async Task<ConferenceValues.Range> GetValuesAsync(Guid hubguid, Guid providerguid, string productCode, string productPriceTable, double? salary, int age, string aggregator, string authorization)
        {

            string uri = $"{_config.GetValue<string>("Endpoints:PriceTable")}/Customer/{hubguid}/Provider/{providerguid}/Product/{productCode}/TableName/{productPriceTable}/getRange?Salary={salary}&Age={age}";

            using var client = new HttpClient();
            client.DefaultRequestHeaders.Add("aggregator", aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            using (HttpResponseMessage response = await client.GetAsync(uri))
            {
                if (response.IsSuccessStatusCode)
                {
                    string json = await response.Content.ReadAsStringAsync();
                    var values = JsonSerializer.Deserialize<ConferenceValues.Range>(json);
                    return values;
                }

                return null;
            }
        }
    }
}