using LoginAppService.Data.VOs.User;
using LoginAppService.Model;
using LoginAppService.Services.Interfaces;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoginAppService.Services
{
	public class UserService : IUserService
	{
		private readonly IConfiguration _configuration;

		public UserService(IConfiguration configuration)
		{
			_configuration = configuration;
		}

		public async Task<UserVO> CreateUserDataAsync(CreateUserVO newUserData)
		{
			string uri = $"{string.Format(_configuration.GetValue<string>("Endpoints:Identity"))}/User/Add";
			HttpClientHandler clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};

			HttpClient client = new HttpClient(clientHandler) { BaseAddress = new Uri(uri) };

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_configuration.GetValue<string>("IdentityAuth:Schema"), _configuration.GetValue<string>("IdentityAuth:Param"));

			var json = JsonSerializer.Serialize(newUserData);
			var data = new StringContent(json, Encoding.UTF8);
			data.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using HttpResponseMessage response = await client.PostAsync("", data);

			if (response.IsSuccessStatusCode)
			{
				string apiResponse = await response.Content.ReadAsStringAsync();

				JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				options.Converters.Add(new JsonStringEnumConverter());
				return JsonSerializer.Deserialize<Response<UserVO>>(apiResponse, options)?.Object;
			}

			return null;
		}

		public async Task<List<UserVO>> GetUsersInfoAsync()
		{
			var clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};

			string uri = $"{string.Format(_configuration.GetValue<string>("Endpoints:Identity"))}/User/Find/All";

			var client = new HttpClient(clientHandler) { BaseAddress = new Uri(uri) };
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_configuration.GetValue<string>("IdentityAuth:Schema"), _configuration.GetValue<string>("IdentityAuth:Param"));

			var response = await client.GetAsync("");
			if (response.IsSuccessStatusCode)
			{
				string result = await response.Content.ReadAsStringAsync();

				JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				options.Converters.Add(new JsonStringEnumConverter());
				return JsonSerializer.Deserialize<Response<List<UserVO>>>(result, options)?.Object;
			}

			return null;
		}

		public async Task<UserVO> BlockUserAsync(Guid userguid)
		{
			string Uri = $"{string.Format(_configuration.GetValue<string>("Endpoints:Identity"))}/User/Block/{userguid}";
			HttpClientHandler clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};

			HttpClient client = new HttpClient(clientHandler)
			{
				BaseAddress = new Uri(Uri)
			};

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_configuration.GetValue<string>("IdentityAuth:Schema"), _configuration.GetValue<string>("IdentityAuth:Param"));

			using HttpResponseMessage response = await client.PatchAsync("", null);

			if (response.IsSuccessStatusCode)
			{
				string apiResponse = await response.Content.ReadAsStringAsync();

				JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				options.Converters.Add(new JsonStringEnumConverter());
				return JsonSerializer.Deserialize<Response<UserVO>>(apiResponse, options)?.Object;
			}

			return null;
		}

		public async Task<UserVO> UnblockUserAsync(Guid userguid)
		{
			string Uri = $"{string.Format(_configuration.GetValue<string>("Endpoints:Identity"))}/User/Unblock/{userguid}";
			HttpClientHandler clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};

			HttpClient client = new HttpClient(clientHandler)
			{
				BaseAddress = new Uri(Uri)
			};

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_configuration.GetValue<string>("IdentityAuth:Schema"), _configuration.GetValue<string>("IdentityAuth:Param"));

			using HttpResponseMessage response = await client.PatchAsync("", null);

			if (response.IsSuccessStatusCode)
			{
				string apiResponse = await response.Content.ReadAsStringAsync();

				JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				options.Converters.Add(new JsonStringEnumConverter());
				return JsonSerializer.Deserialize<Response<UserVO>>(apiResponse, options)?.Object;
			}

			return null;
		}

		public async Task<UserVO> UpdateUserDataAsync(UpdateUserVO newUserData)
		{
			string Uri = $"{string.Format(_configuration.GetValue<string>("Endpoints:Identity"))}/User/Update";
			HttpClientHandler clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};

			HttpClient client = new HttpClient(clientHandler)
			{
				BaseAddress = new Uri(Uri)
			};

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_configuration.GetValue<string>("IdentityAuth:Schema"), _configuration.GetValue<string>("IdentityAuth:Param"));

			var json = JsonSerializer.Serialize(newUserData, new JsonSerializerOptions { IgnoreNullValues = true });
			var data = new StringContent(json, Encoding.UTF8);
			data.Headers.ContentType = new MediaTypeHeaderValue("application/json");

			using HttpResponseMessage response = await client.PutAsync("", data);

			if (response.IsSuccessStatusCode)
			{
				string apiResponse = await response.Content.ReadAsStringAsync();

				JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
				options.Converters.Add(new JsonStringEnumConverter());
				return JsonSerializer.Deserialize<Response<UserVO>>(apiResponse, options)?.Object;
			}

			return null;
		}

		public async Task<bool> DeleteUserAsync(string id)
		{
			string Uri = $"{string.Format(_configuration.GetValue<string>("Endpoints:Identity"))}/User/Remove/{id}";
			HttpClientHandler clientHandler = new HttpClientHandler
			{
				ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
			};

			HttpClient client = new HttpClient(clientHandler)
			{
				BaseAddress = new Uri(Uri)
			};

			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_configuration.GetValue<string>("IdentityAuth:Schema"), _configuration.GetValue<string>("IdentityAuth:Param"));

			using HttpResponseMessage response = await client.DeleteAsync("");

			return response.StatusCode == HttpStatusCode.NoContent;
		}
	}
}