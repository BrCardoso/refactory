using Commons.Models.Operations.WSO2;

using Microsoft.Extensions.Configuration;

using Newtonsoft.Json;

using NotifierAppService.Models;

using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Commons
{
	public class operations
	{
		public static async Task<ApplicationResponseModel> getValidaTokenAsync(string token, IConfiguration config)
		{
			ApplicationResponseModel ret = new ApplicationResponseModel();

			string Uri = string.Format(config.GetValue<string>("Endpoints:WSO2Login"));
			HttpClientHandler clientHandler = new HttpClientHandler();
			clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

			// Pass the handler to httpclient(from you are calling api)
			//using var httpClient = new HttpClient();
			HttpClient client = new HttpClient(clientHandler);

			client.BaseAddress = new System.Uri(Uri);
			client.DefaultRequestHeaders.Accept.Clear();
			client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
			client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(config.GetValue<string>("WSO2AutorizationHeader:Schema"), config.GetValue<string>("WSO2AutorizationHeader:Param"));

			var dict = new Dictionary<string, string>();
			//dict.Add("grant_type", "password");
			dict.Add("token", token);

			HttpResponseMessage response = client.PostAsync(Uri, new FormUrlEncodedContent(dict)).Result;
			string apiResponse = await response.Content.ReadAsStringAsync();

			ret.Success = response.IsSuccessStatusCode;
			if (response.IsSuccessStatusCode)
			{
				var objResponse = JsonConvert.DeserializeObject<IdentityWSO2>(apiResponse);
				ret.credentials = objResponse;

				var handler = new JwtSecurityTokenHandler();
				var jsonToken = handler.ReadToken(ret.credentials.id_token);
				var tokenS = handler.ReadToken(ret.credentials.id_token) as JwtSecurityToken;

				ret.accesslevel = tokenS.Claims.First(claim => claim.Type == "accessLevel").Value;
				ret.accessToBI = tokenS.Claims.First(claim => claim.Type == "accessToBI").Value;
				ret.approveNewLogins = tokenS.Claims.First(claim => claim.Type == "approveNewLogins").Value;
				ret.cnpj = tokenS.Claims.First(claim => claim.Type == "cnpj").Value;
				ret.email = tokenS.Claims.First(claim => claim.Type == "cnpj").Value; ;
				ret.makeLoadRequest = tokenS.Claims.First(claim => claim.Type == "makeLoadRequest").Value;
				ret.family_name = tokenS.Claims.First(claim => claim.Type == "family_name").Value;
				ret.given_name = tokenS.Claims.First(claim => claim.Type == "given_name").Value;

				//ret.customer = hubcustomer.companyguid;
				//ret.customerName = hubcustomer.companyName;
				ret.username = ret.given_name + " " + ret.family_name;
				//ret.group = JsonConvert.SerializeObject(hubcustomer.groups);
				ret.Success = true;
			}
			else
				ret.Message = "Erro ao tentar fazer login.";

			return ret;
		}

		public static async Task<MethodFeedback> PostNotifierAsync(Notifier modelNotifier, IConfiguration config)
		{
			var json = JsonConvert.SerializeObject(modelNotifier);
			var data = new StringContent(json, Encoding.UTF8, "application/json");

			var url = $"{string.Format(config.GetValue<string>("Endpoints:Notifier"))}/Customer/{modelNotifier.hubguid}/aggregator/{modelNotifier.aggregator}";
			using var httpClient = new HttpClient();
			using var response = await httpClient.PostAsync(url, data);

			return new MethodFeedback { Success = response.IsSuccessStatusCode };
		}
	}
}