using Commons.Models.Operations.WSO2;
using Couchbase.N1QL;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Commons
{
    public static class Helpers
    {
        public static class EnumerableStringToUpperCaseConverter
        {
            public static List<string> Parse(IEnumerable<string> values)
            {
                var newValues = new List<string>();

                foreach (var value in values)
                    newValues.Add(value.ToUpper());

                return newValues;
            }
        }
        public static DateTime? ToNullableDate(this String dateString)
        {
            DateTime? ret;
            try
            {
                ret = DateTime.ParseExact(dateString.Trim(),
                                            "yyyyMMdd",
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            System.Globalization.DateTimeStyles.None);

            }
            catch (Exception)
            {
                ret = null;
            }

            return ret;
        }

        public static float? ToFloat(this String floatString)
        {
            float? ret;
            try
            {
                ret = float.Parse(floatString.Replace(",", "."), CultureInfo.InvariantCulture);

            }
            catch (Exception)
            {
                ret = null;
            }

            return ret;
        }
        public static bool ValidaCPF(string cpf)
        {
            if (cpf.Length <= 11)
            {
                while (cpf.Length != 11)
                {
                    cpf = '0' + cpf;
                }

                bool igual = true;
                for (int i = 1; i < 11 && igual; i++)
                {
                    if (cpf[i] != cpf[0])
                    {
                        igual = false;
                    }
                }

                if (igual || cpf == "12345678909")
                {
                    return false;
                }

                int[] numeros = new int[11];

                for (int i = 0; i < 11; i++)
                {
                    numeros[i] = int.Parse(cpf[i].ToString());
                }

                int soma = 0;
                for (int i = 0; i < 9; i++)
                {
                    soma += (10 - i) * numeros[i];
                }

                int resultado = soma % 11;

                if (resultado == 1 || resultado == 0)
                {
                    if (numeros[9] != 0)
                    {
                        return false;
                    }
                }
                else if (numeros[9] != 11 - resultado)
                {
                    return false;
                }

                soma = 0;
                for (int i = 0; i < 10; i++)
                {
                    soma += (11 - i) * numeros[i];
                }

                resultado = soma % 11;

                if (resultado == 1 || resultado == 0)
                {
                    if (numeros[10] != 0)
                    {
                        return false;
                    }
                }
                else
                    if (numeros[10] != 11 - resultado)
                {
                    return false;
                }

                return true;
            }
            else
            {
                int[] digitos, soma, resultado;
                int nrDig; string ftmt;
                bool[] CNPJOk;
                ftmt = "6543298765432";
                digitos = new int[14];
                soma = new int[2];
                soma[0] = 0;
                soma[1] = 0;
                resultado = new int[2];
                resultado[0] = 0;
                resultado[1] = 0;
                CNPJOk = new bool[2];
                CNPJOk[0] = false;
                CNPJOk[1] = false;
                try
                {
                    for (nrDig = 0; nrDig < 14; nrDig++)
                    {
                        digitos[nrDig] = int.Parse(cpf.Substring(nrDig, 1));
                        if (nrDig <= 11)
                        {
                            soma[0] += (digitos[nrDig] * int.Parse(ftmt.Substring(nrDig + 1, 1)));
                        }

                        if (nrDig <= 12)
                        {
                            soma[1] += (digitos[nrDig] * int.Parse(ftmt.Substring(nrDig, 1)));
                        }
                    }

                    for (nrDig = 0; nrDig < 2; nrDig++)
                    {
                        resultado[nrDig] = (soma[nrDig] % 11);
                        if ((resultado[nrDig] == 0) || (resultado[nrDig] == 1))
                        {
                            CNPJOk[nrDig] = (digitos[12 + nrDig] == 0);
                        }
                        else
                        {
                            CNPJOk[nrDig] = (digitos[12 + nrDig] == (11 - resultado[nrDig]));
                        }
                    }

                    return (CNPJOk[0] && CNPJOk[1]);
                }
                catch
                {
                    return false;
                }
            }

        }

        public static bool ValidaCNPJ(string cnpj)
        {
            int[] multiplicador1 = new int[12] { 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int[] multiplicador2 = new int[13] { 6, 5, 4, 3, 2, 9, 8, 7, 6, 5, 4, 3, 2 };
            int soma;
            int resto;
            string digito;
            string tempCnpj;
            //cnpj = cnpj.Trim();
            //cnpj = cnpj.Replace(".", "").Replace("-", "").Replace("/", "");
            if (cnpj.Length != 14)
            {
                return false;
            }

            tempCnpj = cnpj.Substring(0, 12);
            soma = 0;
            for (int i = 0; i < 12; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador1[i];
            }

            resto = (soma % 11);
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = resto.ToString();
            tempCnpj = tempCnpj + digito;
            soma = 0;
            for (int i = 0; i < 13; i++)
            {
                soma += int.Parse(tempCnpj[i].ToString()) * multiplicador2[i];
            }

            resto = (soma % 11);
            if (resto < 2)
            {
                resto = 0;
            }
            else
            {
                resto = 11 - resto;
            }

            digito = digito + resto.ToString();
            return cnpj.EndsWith(digito);
        }

        public static string LimpaString(string texto)
        {
            return Regex.Replace(texto, "[^0-9a-zA-Z]+", "");
        }

        public static string RemoveNaoNumericos(string text)
        {
            System.Text.RegularExpressions.Regex reg = new System.Text.RegularExpressions.Regex(@"[^0-9]");
            string ret = reg.Replace(text, string.Empty);
            return ret;
        }

        public static bool IsNumeric(string p)
        {
            float output;
            return float.TryParse(p, out output);
        }

        public static string ResizeString(string text, int tamanho)
        {
            string ret = text.Trim();
            if (text.Length > tamanho)
            {
                ret = text.Substring(0, tamanho);
            }
            return ret;
        }

        public static bool IsEmail(string strEmail)
        {
            string strModelo = "^([0-9a-zA-Z]([-.\\w]*[0-9a-zA-Z])*@([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,9})$";
            if (System.Text.RegularExpressions.Regex.IsMatch(strEmail, strModelo))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static DateTime? yyyyMMddToDate(string strData)
        {
            DateTime? ret;
            try
            {
                ret = DateTime.ParseExact(strData.Trim(),
                                            "yyyyMMdd",
                                            System.Globalization.CultureInfo.InvariantCulture,
                                            System.Globalization.DateTimeStyles.None);

            }
            catch (Exception)
            {
                ret = null;
            }

            return ret;
        }
        
        public static object TrimObject(object obj)
        {
            if (obj != null)
            {
                var objProperties = obj.GetType().GetProperties();
                //.Where(p => p.PropertyType == typeof(string));

                foreach (var objProperty in objProperties)
                {
                    if (objProperty.PropertyType == typeof(string))
                    {
                        string currentValue = (string)objProperty.GetValue(obj, null);
                        if (!string.IsNullOrEmpty(currentValue))
                        {
                            objProperty.SetValue(obj, currentValue.Trim(), null);
                        }
                    }
                    else if (objProperty.PropertyType.IsGenericType && objProperty.PropertyType.GetGenericTypeDefinition()
                                == typeof(List<>))
                    {
                        var genArgs = objProperty.PropertyType.GetGenericArguments();

                        for (int i = 0; i < genArgs.Count(); i++)
                        {
                            if (genArgs[i].IsClass)
                            {
                                //TrimObject(genArgs[i].GetValue(obj, null));
                            }
                        };
                    }
                    else if (objProperty.PropertyType.IsClass)
                    {
                        objProperty.SetValue(obj, TrimObject(objProperty.GetValue(obj, null)), null);
                    }
                }
            }


            return obj;
        }

        public static void ReadPropertiesRecursive(Type type)
        {
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (property.PropertyType == typeof(string))
                {
                    string currentValue = (string)property.GetValue(type, null);
                    property.SetValue(type, currentValue.Trim(), null);
                }
                else if (property.PropertyType.IsClass)
                {
                    ReadPropertiesRecursive(property.PropertyType);
                }
            }
        }
                     
        [AttributeUsage(
    AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter,
    AllowMultiple = false)]
        public class NotEmptyAttribute : ValidationAttribute
        {
            public const string DefaultErrorMessage = "The {0} field must not be empty";
            public NotEmptyAttribute() : base(DefaultErrorMessage) { }

            public override bool IsValid(object value)
            {
                //NotEmpty doesn't necessarily mean required
                if (value is null)
                {
                    return false;
                }

                switch (value)
                {
                    case Guid guid:
                        return guid != Guid.Empty;
                    default:
                        return true;
                }
            }
        }


        [AttributeUsage(
        AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter)]
        public sealed class ListCannotBeEmptyAttribute : ValidationAttribute
        {
            private const string DefaultErrorMessage = "'{0}' must have at least one element.";
            public ListCannotBeEmptyAttribute() : base(DefaultErrorMessage) { }

            public override bool IsValid(object value)
            {
                IList list = value as IList;
                return (list != null && list.Count > 0);
            }

            public override string FormatErrorMessage(string name)
            {
                return String.Format(this.ErrorMessageString, name);
            }
        }

        public static void NEVER_EAT_POISON_Disable_CertificateValidation()
        {
            // Disabling certificate validation can expose you to a man-in-the-middle attack
            // which may allow your encrypted message to be read by an attacker
            // https://stackoverflow.com/a/14907718/740639
            ServicePointManager.ServerCertificateValidationCallback =
                delegate (
                    object s,
                    X509Certificate certificate,
                    X509Chain chain,
                    SslPolicyErrors sslPolicyErrors
                ) {
                    return true;
                };
        }
        public static bool ValidateServerCertificate(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            if (sslPolicyErrors == SslPolicyErrors.None)
                return true;
            else
            {
                return true;                
            }
        }

        public static async Task<Login.ApplicationResponseModel> ReValidateUser(Auth.RefreshTokenModel refreshToken, IConfiguration _config)
        {
            var ret = new Login.ApplicationResponseModel();
            try
            {
                string Uri = string.Format(_config.GetValue<string>("Endpoints:WSO2Refresh"));
                HttpClientHandler clientHandler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; }
                };

                HttpClient client = InitClientWSO2(Uri, clientHandler, _config);

                var dict = new Dictionary<string, string>
                    {
                        { "grant_type", "refresh_token" },
                        { "refresh_token", refreshToken.refreshToken},
                    };

                HttpResponseMessage response = await client.PostAsync(Uri, new FormUrlEncodedContent(dict));
                string apiResponse = await response.Content.ReadAsStringAsync();

                ret = new Login.ApplicationResponseModel
                {
                    Success = response.IsSuccessStatusCode
                };
                if (response.IsSuccessStatusCode)
                {
                    var objResponse = JsonConvert.DeserializeObject<Login.WSO2ResponseModel>(apiResponse);
                    ret.credentials = objResponse;

                    var handler = new JwtSecurityTokenHandler();
                    var jsonToken = handler.ReadToken(ret.credentials.id_token);
                    var tokenS = handler.ReadToken(ret.credentials.id_token) as JwtSecurityToken;

                    
                    ret.accesslevel = tokenS.Claims.First(claim => claim.Type == "accessLevel").Value;
                    ret.accessToBI = tokenS.Claims.First(claim => claim.Type == "accessToBI").Value;
                    ret.approveNewLogins = tokenS.Claims.First(claim => claim.Type == "approveNewLogins").Value;
                    ret.cnpj = tokenS.Claims.First(claim => claim.Type == "cnpj").Value;
                    ret.email = tokenS.Claims.First(cliam => cliam.Type == "sub").Value;
                    ret.makeLoadRequest = tokenS.Claims.First(claim => claim.Type == "makeLoadRequest").Value;
                    ret.family_name = tokenS.Claims.First(claim => claim.Type == "family_name").Value;
                    ret.given_name = tokenS.Claims.First(claim => claim.Type == "given_name").Value;

                    //var hubcustomer = await operations.getHubCustomerAsync(ret.cnpj, ret.accesslevel, _config);
                    //if (hubcustomer.Success)
                    //{
                    //    ret.customer = hubcustomer.companyguid;
                    //    ret.customerName = hubcustomer.companyName;
                    //    ret.username = ret.given_name + " " + ret.family_name;
                    //    ret.group = JsonConvert.SerializeObject(hubcustomer.groups);
                    //    ret.Success = true;
                    //}
                    //else
                    //    return BadRequest("Erro ao tentar Localizar contrato da empresa.");
                }
                else
                {
                    ret.Success = false; ret.Message = "Erro de validação do usuário/token.";
                }

            }
            catch (Exception ex)
            {
                ret.Success = false; ret.Exception = true; ret.Message = ex.ToString();
            }
        
            return ret;
        }

        private static HttpClient InitClientWSO2(string Uri, HttpClientHandler clientHandler,IConfiguration _config)
        {
            HttpClient client = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri(Uri)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(_config.GetValue<string>("WSO2AutorizationHeader:Schema"), _config.GetValue<string>("WSO2AutorizationHeader:Param"));
            return client;
        }
        private static HttpClient InitClient(string Uri, HttpClientHandler clientHandler, string aggregator)
        {
            HttpClient client = new HttpClient(clientHandler)
            {
                BaseAddress = new Uri(Uri)
            };
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            client.DefaultRequestHeaders.Add("aggregator", aggregator);
            return client;
        }

        public static async Task<MethodFeedback> Valida(IPAddress iPAddress, string authorization, string wso2refreshtoken, string aggregator, IConfiguration _config) {
            MethodFeedback mf = new MethodFeedback();
            try
            {
                if (string.IsNullOrEmpty(aggregator))
                    return new MethodFeedback { Success = false, Message = "Missing header aggregator.", HttpStatusCode = 401 };

                //valida o header authorization access token wso2
                if (string.IsNullOrEmpty(authorization))
                    return new MethodFeedback { Success = false, Message = "Missing header Authorization.", HttpStatusCode = 401 };
                
                ////valida o header refresh_token wso2
                //if (string.IsNullOrEmpty(wso2refreshtoken))
                //    return new MethodFeedback { Success = false, Message = "Missing header refresh_token.", HttpStatusCode = 401 };

                //valida jwt
                var bearer = authorization.Split(" ");
                var jwt = bearer[1];
                var result = validaJWT(jwt);
                if (result != null)
                {
                    if (aggregator.Contains(result.accesslevel))
                        return new MethodFeedback { Success = true, Message = wso2refreshtoken };       //passou             
                    else
                        return new MethodFeedback { Success = false, Message = "Usuário não tem acesso ao aggregator solicitado.", HttpStatusCode = 401 };
                }
                else
                    return new MethodFeedback { Success = false, Message = "Acesso expirado.", HttpStatusCode = 401 };
                
                ////revalida acesso no wso2
                //var validateUser = await ReValidateUser(new Auth.RefreshTokenModel { refreshToken = wso2refreshtoken }, _config);
                //if (validateUser.Success)
                //{
                //    if (aggregator.Contains(validateUser.accesslevel))
                //        return new MethodFeedback { Success = true, Message = validateUser.credentials.refresh_token };       //passou             
                //    else
                //        return new MethodFeedback { Success = false, Message = "Usuário não tem acesso ao aggregator solicitado.", HttpStatusCode = 401 };                    
                //}
                //else
                //    return new MethodFeedback { Success = false, Message = validateUser.Message, HttpStatusCode = 401 };

            }
            catch (Exception ex)
            {
                return new MethodFeedback { Success = false, Message = ex.ToString(), HttpStatusCode = 500 };
            }
        }

        private static Login.ApplicationResponseModel validaJWT(string jwt)
        {
            try
            {
                var ret = new Login.ApplicationResponseModel();
                var handler = new JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(jwt);
                var tokenS = handler.ReadToken(jwt) as JwtSecurityToken;

                ret.accesslevel = tokenS.Claims.First(claim => claim.Type == "accessLevel").Value;
                ret.accessToBI = tokenS.Claims.First(claim => claim.Type == "accessToBI").Value;
                ret.approveNewLogins = tokenS.Claims.First(claim => claim.Type == "approveNewLogins").Value;
                ret.cnpj = tokenS.Claims.First(claim => claim.Type == "cnpj").Value;
                ret.email = tokenS.Claims.First(cliam => cliam.Type == "sub").Value;
                ret.makeLoadRequest = tokenS.Claims.First(claim => claim.Type == "makeLoadRequest").Value;
                ret.family_name = tokenS.Claims.First(claim => claim.Type == "family_name").Value;
                ret.given_name = tokenS.Claims.First(claim => claim.Type == "given_name").Value;

                return ret;

            }
            catch (Exception)
            {
                return null;
            }
                        
        }
    }   
}
