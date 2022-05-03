using Commons;
using Commons.Enums;

using LoginAppService.Model;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

using Newtonsoft.Json;

using System;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoginAppService.Controler
{
    [Route("api/v1/[controller]")]
    [ApiController]
    public class LoginController : ControllerBase
    {
        private readonly ILogger<LoginController> _logger;
        private readonly IConfiguration _config;

        public LoginController(ILogger<LoginController> logger, IConfiguration config)
        {
            _logger = logger;
            _config = config;
        }

        [HttpGet]
        [Route("GetModel")]
        public object GetModel()
        {
            var model = new Login.RequestModel();
            return Ok(model);
        }

        [HttpPost]
        [ProducesResponseType(typeof(Login.ApplicationResponseModel), 200)]
        public async Task<IActionResult> SignInAsync([FromBody] Login.RequestModel model)
        {
            Login.ApplicationResponseModel ret = new Login.ApplicationResponseModel();
            try
            {
                if (ModelState.IsValid)
                {
                    string Uri = $"{string.Format(_config.GetValue<string>("Endpoints:Identity"))}/Auth/SignIn";
                    HttpClientHandler clientHandler = new HttpClientHandler();
                    clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };

                    HttpClient client = new HttpClient(clientHandler);

                    client.BaseAddress = new Uri(Uri);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                    client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_config.GetValue<string>("IdentityAuth:Schema"), _config.GetValue<string>("IdentityAuth:Param"));

                    var json = JsonConvert.SerializeObject(new { Email = model.login, Password = model.password });
                    var data = new StringContent(json, Encoding.UTF8);
                    data.Headers.ContentType = new MediaTypeHeaderValue("application/json");

                    HttpResponseMessage response = await client.PostAsync(Uri, data);
                    string apiResponse = await response.Content.ReadAsStringAsync();
                    JsonSerializerOptions options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                    options.Converters.Add(new JsonStringEnumConverter());
                    var objResponse = System.Text.Json.JsonSerializer.Deserialize<Response<Login.IdentityRequestModel>>(apiResponse, options);

                    ret.Success = response.IsSuccessStatusCode;
                    if (response.IsSuccessStatusCode)
                    {
                        ret.credentials = new Login.IdentityResponseModel
                        {
                            access_token = objResponse.Object.accessToken,
                            expires_in = objResponse.Object.expiresIn,
                            id_token = objResponse.Object.token,
                            refresh_token = objResponse.Object.refreshToken
                        };

                        var handler = new JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadToken(ret.credentials.id_token);
                        var tokenS = handler.ReadToken(ret.credentials.id_token) as JwtSecurityToken;

                        ret.accesslevel = tokenS.Claims.First(claim => claim.Type == "accessLevel").Value;
                        ret.accessToBI = tokenS.Claims.First(claim => claim.Type == "accessToBI").Value.ToLower() == "true";
                        ret.approveNewLogins = tokenS.Claims.First(claim => claim.Type == "approveNewLogins").Value.ToLower() == "true";
                        ret.makeLoadRequest = tokenS.Claims.First(claim => claim.Type == "makeLoadRequest").Value.ToLower() == "true";
                        ret.changeOtherUsers = tokenS.Claims.First(claim => claim.Type == "changeOtherUsers").Value.ToLower() == "true";
                        ret.makeInvoiceConference = tokenS.Claims.First(claim => claim.Type == "makeInvoiceConference").Value.ToLower() == "true";
                        ret.cnpj = tokenS.Claims.First(claim => claim.Type == "cnpj").Value;
                        ret.email = model.login;
                        ret.family_name = tokenS.Claims.First(claim => claim.Type == "family_name").Value;
                        ret.given_name = tokenS.Claims.First(claim => claim.Type == "given_name").Value;
                        ret.typeUser = tokenS.Claims.First(claim => claim.Type == "userType").Value;

                        var hubcustomer = await operations.getHubCustomerAsync(ret.cnpj, ret.accesslevel, _config);
                        
                        if (ret.typeUser.ToUpper() == "SYSADMIN")
                            return Ok(ret);

                        if (hubcustomer.Success)
                        {
                            if (hubcustomer?.Status.ToUpper() == "ATIVO")
                            {
                                ret.customer = hubcustomer.companyguid;
                                ret.customerName = hubcustomer.companyName;
                                ret.username = ret.given_name + " " + ret.family_name;
                                ret.group = JsonConvert.SerializeObject(hubcustomer.groups);
                                ret.Success = true;
                            }
                            else
                                return BadRequest(new MethodFeedback { Message = "Contrato da empresa bloqueado/inativo.", Success = false, MessageCode = "LOGIN_COMPANY_NOT_ACTIVE" });
                        }
                        else
                            return BadRequest(new MethodFeedback { Message = "Erro ao tentar Localizar contrato da empresa.", Success = false, MessageCode = "LOGIN_COMPANY_CONTRACT_NOT_FOUND" });
                    }
                    else if (response.StatusCode == HttpStatusCode.BadRequest)
                    {
                        if (objResponse.MessageCode == MessageCode.AUTH_USER_NOT_FOUND)
                            return BadRequest(new MethodFeedback { Message = "Login ou senha inválido.", Success = false, MessageCode = "LOGIN_INCORRECT_EMAIL_OR_PASSWORD" });
                        else if (objResponse.MessageCode == MessageCode.USER_BLOCKED)
                            return BadRequest(new MethodFeedback { Message = "Este usuario está atualmente bloqueado.", Success = false, MessageCode = "LOGIN_USER_IS_BLOCKED" });
                    }
                    else
                        return BadRequest(new MethodFeedback { Message = "Erro ao tentar fazer login.", Success = false, MessageCode = objResponse.MessageCode.ToString() });

                    return Ok(ret);
                }
                else
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors);
                    return BadRequest(JsonConvert.SerializeObject(errors));
                }
            }
            catch (Exception ex)
            {
                //em caso de falha
                _logger.LogInformation(ex.ToString(), ex);
                return BadRequest(ex.ToString());
            }
        }

        [HttpOptions()]
        public void Options()
        {
        }
    }
}