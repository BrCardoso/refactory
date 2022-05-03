using BeneficiaryAppService.ServiceRequest.Interfaces;

using Commons.Models;

using Microsoft.Extensions.Configuration;

using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BeneficiaryAppService.ServiceRequest
{
	public class EventRequestService : IEventRequestService
	{
		public IConfiguration _config { get; set; }

		public EventRequestService(IConfiguration config)
		{
			_config = config;
		}

		public async Task<Event> CreateEventAsync(Event newEvent, string authorization)
		{
			string Uri = $"{_config.GetValue<string>("Endpoints:Events")}/Customer/{newEvent.hubguid}";

			var json = JsonSerializer.Serialize(newEvent);
			var data = new StringContent(json, Encoding.UTF8, "application/json");
			using var httpClient = new HttpClient();

			httpClient.DefaultRequestHeaders.Add("aggregator", newEvent.aggregator);
			httpClient.DefaultRequestHeaders.Add("Authorization", $"{authorization}");

			var response = await httpClient.PostAsync(Uri, data);
			if (response.IsSuccessStatusCode)
			{
				return JsonSerializer.Deserialize<Event>(await response.Content.ReadAsStringAsync());
			}

			return null;
		}
	}
}