using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using System.Net.Http;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using Commons.Base;
using Couchbase.Core;
using Couchbase.N1QL;
using Commons;
using JobsMicroservice.Model;

namespace JobsMicroservice
{
    public class operations
    {
        public static async Task<MethodFeedback> HandleFileAsync(Guid token, QueueModel model, IConfiguration config, IBucket bucket, string authorization)
        {
            Commons.MethodFeedback mf = new Commons.MethodFeedback();
            try
            {
                //busca dados do cliente hub
                var customer = new List<HubCustomerOut>();
                var HubContractRequest = await getHubCustomerAsync(model.hubguid, config);
                if (HubContractRequest.Success)
                {
                    customer = JsonConvert.DeserializeObject<List<HubCustomerOut>>(HubContractRequest.Message);
                    //referencia de hierarquia dentro da organização;
                }
                else
                {
                    mf.Success = false;
                    mf.Message = "Não localizamos os dados da empresa.";
                    return mf;
                }

                //busca dados do provedor
                var provider = new List<ProviderDB>();
                var prov = await getProviderAsync(model.providerguid, config);
                if (prov.Success)
                {
                    provider = JsonConvert.DeserializeObject<List<ProviderDB>>(prov.Message);
                }
                else
                {
                    mf.Success = false;
                    mf.Message = "Não localizamos os dados do provedor.";
                    return mf;
                }

                //busca dados do produto
                var produto = provider[0].Products.Where(x => x.Providerproductcode.ToUpper() == model.beneficiary[0].benefitinfos.providerproductcode.ToUpper()).FirstOrDefault();

                //busca dados da empresa
                var company = new List<CompanyDB>();
                var comp = await getCompanyAsync(model.aggregator.Split("-")[2], config);
                if (comp.Success)
                {
                    company = JsonConvert.DeserializeObject<List<CompanyDB>>(comp.Message);
                }
                else
                {
                    mf.Success = false;
                    mf.Message = "Não localizamos os dados do provedor.";
                    return mf;
                }

                //busca dados do provedorStruc
                var findProviderStruc = new List<ProviderStrucDB>();
                var provStruc = await getProviderStrucAsync(model.providerguid, model.hubguid, model.aggregator, config, authorization);
                if (provStruc.Success)
                {
                    findProviderStruc = JsonConvert.DeserializeObject<List<ProviderStrucDB>>(provStruc.Message);
                }

                //busca dados do Contrato
                var findRuleConfig = getRC(token, null, model.providerguid, model.linkinformation.contractnumber, bucket);


                //inclui no couchbase
                model.linkinformation = new LinkInformation
                {
                    costumernumber = findProviderStruc[0].accesscredentials.Costumernumber,
                    login = findProviderStruc[0].accesscredentials.Login,
                    password = findProviderStruc[0].accesscredentials.Password,
                    contractissued = findRuleConfig.contractIssued,
                    contractnumber = findRuleConfig.contractnumber,
                    responsibleperson = findRuleConfig.hrresponsable.name,
                    responsibleid = findRuleConfig.hrresponsable.CPF,
                    responsiblebirthdate = findRuleConfig.hrresponsable.birthdate
                };
                model.beneficiary[0].benefitinfos = new BenefitInfos
                {
                    providerproductcode = model.beneficiary[0].benefitinfos.providerproductcode,
                    product = produto.Description,
                    subsegment = produto.Subsegcode,
                    cardnumber = model.beneficiary[0].benefitinfos.cardnumber,
                    startdate = DateTime.Now,
                    reissuereason = model.beneficiary[0].benefitinfos.reissuereason
                };
                if (model.beneficiary[0].holder.jobinfo == null)
                {
                    //busca dados do Funcionario
                    var Employee = new Commons.Base.Employeeinfo();
                    var findEmployeeInfo = await getEmployeeInfoAsync(model.beneficiary[0].holder.personguid, model.hubguid, model.aggregator, config, authorization);
                    if (findEmployeeInfo.Success)
                    {
                        Employee = JsonConvert.DeserializeObject<List<Commons.Base.Employeeinfo>>(provStruc.Message)[0];
                    }
                    model.beneficiary[0].holder.jobinfo = new EmployeeinfoClean
                    {
                        Admissiondate = Employee.Admissiondate,
                        Costcenter = Employee.Costcenter,
                        Costcentercode = Employee.Costcentercode,
                        Department = Employee.Department,
                        Departmentcode = Employee.Departmentcode,
                        Employeecomplementaryinfos = Employee.Employeecomplementaryinfos,
                        Functionalcategory = Employee.Functionalcategory,
                        Functionalcategorycode = Employee.Functionalcategorycode,
                        Occupation = Employee.Occupation,
                        Occupationcode = Employee.Occupationcode,
                        Registration = Employee.Registration,
                        Role = Employee.Role,
                        Rolecode = Employee.Rolecode,
                        Salary = Employee.Salary,
                        Shift = Employee.Shift,
                        Union = Employee.Union,
                        Unioncode = Employee.Unioncode
                    };
                }
                
                if (model.guid == Guid.Empty)
                {
                    model.guid = Guid.NewGuid();
                    model.incdate = DateTime.Now;
                    model.status = "Aberto";
                }
                var a = bucket.Upsert(
                    model.guid.ToString(), new
                    {
                        model.guid,
                        docType = "Movement",
                        model.incdate,
                        model.hubguid,
                        model.aggregator,
                        model.providerguid,
                        model.movementtype,
                        model.status,
                        model.linkinformation,
                        model.beneficiary,
                        model.charge

                    });
                if (a.Success)
                {
                    var objAggreg = customer[0].Hierarchy.Groups.Where(x => x.Companies.Any(y => y.Branches.Contains(company[0].companyid))).Select(o => new { Group = o, Company = o.Companies.Where(d => d.Branches.Contains(company[0].companyid)).ToList() }).ToList();

                    string group = objAggreg[0].Group.Name;
                    for (int i = 0; i < model.beneficiary.Count; i++)
                    {
                        var person = new List<Person>();
                        var personRequest = await getPersonAsync(model.beneficiary[i].personguid, config);
                        if (personRequest.Success)
                        {
                            person = JsonConvert.DeserializeObject<List<Person>>(personRequest.Message);

                            model.beneficiary[i].Name = person[0].Name;
                            model.beneficiary[i].Cpf = person[0].Cpf;
                            model.beneficiary[i].Birthdate = person[0].Birthdate;
                            model.beneficiary[i].Gender = person[0].Gender;
                            model.beneficiary[i].Maritalstatus = person[0].Maritalstatus;
                            model.beneficiary[i].Mothername = person[0].Mothername;
                            model.beneficiary[i].Rg = person[0].Rg;
                            model.beneficiary[i].Issuingauthority = person[0].Issuingauthority;
                            model.beneficiary[i].complementaryinfos = person[0].complementaryinfos;
                            model.beneficiary[i].documents = person[0].documents;
                            model.beneficiary[i].Addresses = person[0].Addresses;
                            model.beneficiary[i].Phoneinfos = person[0].Phoneinfos;
                            model.beneficiary[i].emailinfos = person[0].emailinfos;
                        }
                        else
                        {
                            mf.Success = false;
                            mf.Message = "Não localizamos os dados da empresa.";
                            return mf;
                        }
                    }

                    var NITModel = new NitModel
                    {
                        movementtype = model.movementtype,
                        movementguid = token,
                        beneficiary = model.beneficiary,
                        charge = model.charge,
                        linkinformation = model.linkinformation,
                        provider = new Model.Provider
                        {
                            cnpj = provider[0].Cnpj,
                            //Email = provider[0].mail
                            name = provider[0].Description,
                            providerguid = provider[0].guid,
                            providerregistrationcode = provider[0].Registration,
                            segment = provider[0].Segcode,
                            site = provider[0].Site
                        },
                        customer = new Customer
                        {
                            address = company[0].Addresses,
                            aggregator = model.aggregator,
                            branchname = company[0].Branchname,
                            companyid = company[0].companyid,
                            companyname = company[0].TradingName,
                            group = group
                        }
                    };

                    return await PostNITAsync(NITModel, config);
                }
                else
                {
                    return new MethodFeedback { Exception = false, Message = a.Message + " - " + a.Exception?.ToString(), Success = false };
                }
            }
            catch (Exception ex)
            {

                mf.Success = false;
                mf.Exception = true;
                mf.Message = ex.ToString();
                return mf;
            }

        }
        public static async Task<MethodFeedback> getProviderStrucAsync(Guid GUID, Guid id, string aggregator, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:ProviderStruc") + "/Customer/{0}/Provider/{1}", GUID, id);
            using var httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
            httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                MethodFeedback ret = new MethodFeedback();
                ret.Success = response.IsSuccessStatusCode;
                ret.Message = apiResponse;
                return ret;
            }
        }
        private static async Task<Commons.MethodFeedback> PostNITAsync(NitModel itemArq, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:NIT"));
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent.ToLower(), System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();

            Commons.MethodFeedback ret = new Commons.MethodFeedback();
            ret.Success = response.IsSuccessStatusCode;
            if (response.IsSuccessStatusCode)
            {
                var ret1 = JsonConvert.DeserializeObject<NITResponse>(apiResponse);
                ret.obj = ret1;
            }
            else
                ret.Message = "Erro ao tentar registrar a solicitação.";

            return ret;
        }
        private static async Task<MethodFeedback> getHubCustomerAsync(Guid token, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:HubCustomer") + "/{0}", token);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                MethodFeedback ret = new MethodFeedback();
                ret.Success = response.IsSuccessStatusCode;
                ret.Message = apiResponse;

                return ret;
            }
        }
        public static async Task<MethodFeedback> getEmployeeInfoAsync(Guid employeeguid, Guid hubguid, string aggregator, IConfiguration config, string authorization)
        {
            try
            {
                string Uri = string.Format(config.GetValue<string>("Endpoints:Employees") + "/Customer/{0}/Aggregator/{1}", hubguid, aggregator);
                using var httpClient = new HttpClient();
                httpClient.DefaultRequestHeaders.Add("aggregator", aggregator);
                httpClient.DefaultRequestHeaders.Add("Authorization", authorization);
                using (var response = await httpClient.GetAsync(Uri))
                {
                    string apiResponse = await response.Content.ReadAsStringAsync();

                    MethodFeedback ret = new MethodFeedback();
                    ret.Success = response.IsSuccessStatusCode;
                    ret.Message = apiResponse;

                    return ret;
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                return null;
            }
        }
        public static async Task<MethodFeedback> getProviderAsync(Guid GUID, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Provider") + "/{0}", GUID);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                MethodFeedback ret = new MethodFeedback();
                ret.Success = response.IsSuccessStatusCode;
                ret.Message = apiResponse;
                return ret;
            }
        }
        private static async Task<MethodFeedback> getCompanyAsync(string cnpj, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Company") + "/CompanyId/{0}", Commons.Helpers.RemoveNaoNumericos(cnpj));
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();


                Uri = string.Format(config.GetValue<string>("Endpoints:Company") + "/{0}", apiResponse);
                using var httpClient1 = new HttpClient();
                using (var response1 = await httpClient1.GetAsync(Uri))
                {
                    string apiResponse1 = await response1.Content.ReadAsStringAsync();

                    MethodFeedback ret = new MethodFeedback();
                    ret.Success = response1.IsSuccessStatusCode;
                    ret.Message = apiResponse1;
                    return ret;
                }
            }
        }
        private static async Task<MethodFeedback> getPersonAsync(Guid guid, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Person") + "/{0}", guid);
            using var httpClient = new HttpClient();
            using (var response = await httpClient.GetAsync(Uri))
            {
                string apiResponse = await response.Content.ReadAsStringAsync();

                MethodFeedback ret = new MethodFeedback();
                ret.Success = response.IsSuccessStatusCode;
                ret.Message = apiResponse;
                return ret;
            }
        }
        public static RulesConfigurationModel getRC(Guid hubguid, Guid? docid, Guid providerguid, string contractnumber, IBucket _bucket)
        {
            try
            {
                var query = new QueryRequest(
                    @"SELECT g.* FROM DataBucket001 g 
        where g.docType = 'RulesConfiguration' 
        and g.hubguid = $hubguid
        and g.guid = $guid
        or
        g.docType = 'RulesConfiguration' 
        and g.hubguid = $hubguid
        and g.providerguid = $providerguid
        and g.contractnumber = $contract
        ")
                    .AddNamedParameter("$hubguid", hubguid)
                    .AddNamedParameter("$guid", docid)
                    .AddNamedParameter("$providerguid", providerguid)
                    .AddNamedParameter("$contract", contractnumber);

                query.ScanConsistency(ScanConsistency.RequestPlus);
                var result = _bucket.Query<RulesConfigurationModel>(query);
                if (result.Success && result.Rows.Count > 0)
                {
                    return result.Rows[0];
                }
                else
                {
                    return null;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }
    }
}
