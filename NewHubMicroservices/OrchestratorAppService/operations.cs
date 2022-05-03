using Commons.Base;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using NetCoreJobsMicroservice.Models.Base.Output;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Linq;
using Microsoft.Extensions.Logging;
using NetCoreJobsMicroservice.Controler;
using Couchbase.Core;
using Couchbase.N1QL;
using Commons;
using NotifierAppService.Models;

namespace NetCoreJobsMicroservice
{
    public class operations
    {
        public static async Task HandleFileAsync(CustomerFull customerFull, IConfiguration config, Guid eventToken, ILogger<OrchestratorController> _logger, IBucket _bucket, string authorization, string _aggregator)
        {
            _ = Task.Run(() =>
            {
                operations.UpsertLog(eventToken, customerFull.guid,
                    "Em processamento",
                    new List<LogList> { new LogList { Title = "Inicio da leitura do objeto", Success = true } },
                    _bucket,
                    _logger);
            });

            try
            {
                //Localiza cadastro do cliente do HuB
                var HubContractRequest = await getHubCustomerAsync(customerFull.guid, config);
                if (HubContractRequest.Success)
                {
                    //guarda na variavel
                    var HubContract = JsonConvert.DeserializeObject<List<HubCustomerOut>>(HubContractRequest.Message);
                    if (HubContract.Count > 0)
                    {
                        _ = Task.Run(() =>
                        {
                            operations.UpsertLog(eventToken,
                                customerFull.guid,
                                "Em processamento",
                                new List<LogList> { new LogList { Title = "Contrato localizado", Success = true } },
                                _bucket,
                                _logger);
                        });

                        HubCustomerOut hubcontract = new HubCustomerOut
                        {
                            Guid = HubContract[0].Guid,
                            ContractIssued = HubContract[0].ContractIssued,
                            ContractNumber = HubContract[0].ContractNumber,
                            Companies = new List<HubCustomerOut.CompanyOut>()
                        };

                        //verifica se o contrato com o hub esta ativo
                        var objHubContract = HubContract[0];
                        if (objHubContract.Status.ToUpper() == "ATIVO")
                        {
                            _ = Task.Run(() =>
                            {
                                operations.UpsertLog(eventToken,
                                    customerFull.guid,
                                    "Em processamento",
                                    new List<LogList> { new LogList { Title = "Contrato ativo", Success = true } },
                                    _bucket,
                                    _logger);
                            });

                            if (customerFull.Companies != null)
                            {
                                //varre objeto com as novas informações na serem inputadas no sistema
                                foreach (var companyEntrada in customerFull.Companies)
                                {
                                    _ = Task.Run(() =>
                                    {
                                        operations.UpsertLog(eventToken,
                                            customerFull.guid,
                                            "Em processamento",
                                            new List<LogList> { new LogList { Title = "Inicio empresa", Message = "CNPJ:" + companyEntrada.companyid, Success = true } },
                                            _bucket,
                                            _logger);
                                    });

                                    //referencia de hierarquia dentro da organização;
                                    var objAggreg = objHubContract.Hierarchy.Groups.Where(x => x.Companies.Any(y => y.Branches.Contains(companyEntrada.companyid))).Select(o => new { Group = o, Company = o.Companies.Where(d => d.Branches.Contains(companyEntrada.companyid)).ToList() }).ToList();

                                    if (objAggreg.Count > 0)
                                    {
                                        //conseguiu calcular o aggregatr da unidade
                                        string aggregador = objAggreg[0].Group.Code + "-" + objAggreg[0].Company[0].Code + "-" + companyEntrada.companyid;

                                        if (_aggregator.Split("-").Length == 3)
                                        {
                                            aggregador = _aggregator;
                                        }
                                        
                                        //acha a referencia da empresa
                                        var companyresultRequest = await PostCompanyAsync((CompanyOut)companyEntrada, config);
                                        if (companyresultRequest.Success)
                                        {
                                            CompanyOut currentcompanyresult = new CompanyOut();
                                            currentcompanyresult = JsonConvert.DeserializeObject<CompanyOut>(companyresultRequest.Message);

                                            _ = Task.Run(() =>
                                            {
                                                operations.UpsertLog(eventToken,
                                                    customerFull.guid,
                                                    "Em processamento",
                                                    new List<LogList> {
                                                        new LogList { Title = "Empresa incluida/atualizada", Message = "CNPJ:" + currentcompanyresult.companyid, Success = true
                                                        } },
                                                    _bucket,
                                                    _logger);
                                            });

                                            #region Entidades
                                            if (companyEntrada.Entities != null)
                                            {
                                                //inclui entidades
                                                EntityOut entityOut = (EntityOut)companyEntrada.Entities;
                                                entityOut.hubguid = customerFull.guid;
                                                entityOut.aggregator = aggregador;

                                                //registra log de sucesso ou falha na inclusão das entidades
                                                if (PostEntityAsync(entityOut, config, authorization).Result)
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Entidades cadastradas", Message = "CNPJ:" + companyEntrada.companyid, Success = true
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                                else
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Falha ao incluir entidadaes", Message = "CNPJ:" + companyEntrada.companyid, Success = false
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                            }
                                            #endregion

                                            #region Coparticipação
                                            if (companyEntrada.Copart != null)
                                            {
                                                //inclui copart
                                                CopartOut copartOut = (CopartOut)companyEntrada.Copart;
                                                copartOut.hubguid = customerFull.guid;
                                                copartOut.aggregator = aggregador;
                                                //registra log de sucesso ou falha na inclusão das copart
                                                if (PostCopartAsync(copartOut, config, authorization).Result)
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Coparticipação cadastradas", Message = "CNPJ:" + companyEntrada.companyid, Success = true
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                                else
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Falha ao incluir coparticipação", Message = "CNPJ:" + companyEntrada.companyid, Success = false
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                            }
                                            #endregion

                                            #region Sinistralidade
                                            if (companyEntrada.Insuranceclaim != null)
                                            {
                                                //inclui sinistralidade
                                                InsuranceclaimOut insuranceclaimOut = (InsuranceclaimOut)companyEntrada.Insuranceclaim;
                                                insuranceclaimOut.hubguid = customerFull.guid;
                                                insuranceclaimOut.aggregator = aggregador;

                                                //registra log de sucesso ou falha na inclusão das copart
                                                if (PostInsuranceClaimAsync(insuranceclaimOut, config, authorization).Result)
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Sinistralidade cadastradas", Message = "CNPJ:" + companyEntrada.companyid, Success = true
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                                else
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Falha ao incluir sisnistralidade", Message = "CNPJ:" + companyEntrada.companyid, Success = false
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                            }
                                            #endregion

                                            #region Provider_Products
                                            if (companyEntrada.Providers != null)
                                            {
                                                //inclui provedores e produtos
                                                foreach (var item in companyEntrada.Providers)
                                                {
                                                    if (!string.IsNullOrEmpty(item.Segcode))
                                                        item.Segcode = item.Segcode.ToUpper();
                                                    if (string.IsNullOrEmpty(item.Code))
                                                        item.Code = "null";
                                                    if (string.IsNullOrEmpty(item.Registration))
                                                        item.Registration = "null";

                                                    #region providerStruc
                                                    var prov = await PostProviderAsync((Provider)item, config);
                                                    if (prov.Success)
                                                    {
                                                        _ = Task.Run(() =>
                                                        {
                                                            operations.UpsertLog(eventToken,
                                                                customerFull.guid,
                                                                "Em processamento",
                                                                new List<LogList> {
                                                        new LogList { Title = "Provedor localizado", Message = string.Format("CNPJ empresa: {0} CNPJ provedor:{1}", companyEntrada.companyid, item.Cnpj), Success = true
                                                        } },
                                                                _bucket,
                                                                _logger);
                                                        });
                                                        ProviderOut currentProvider = new ProviderOut();
                                                        currentProvider = JsonConvert.DeserializeObject<ProviderOut>(prov.Message);
                                                        Guid provGuid = currentProvider.guid;

                                                        ProviderCustomerOut providerOut = (ProviderCustomerOut)item;
                                                        providerOut.hubguid = customerFull.guid;
                                                        providerOut.aggregator = aggregador;
                                                        providerOut.providerguid = provGuid;

                                                        //Inclui o provider structure no db
                                                        if (await PostProviderStrucAsync(providerOut, config, authorization))
                                                        {
                                                            _ = Task.Run(() =>
                                                            {
                                                                operations.UpsertLog(eventToken,
                                                                    customerFull.guid,
                                                                    "Em processamento",
                                                                    new List<LogList> {
                                                        new LogList { Title = "Dados do provedor cadastrado", Message = string.Format("CNPJ empresa: {0} CNPJ provedor:{1}", companyEntrada.companyid, item.Cnpj), Success = true
                                                        } },
                                                                    _bucket,
                                                                    _logger);
                                                            });
                                                        }
                                                        else
                                                        {
                                                            _ = Task.Run(() =>
                                                            {
                                                                operations.UpsertLog(eventToken,
                                                                    customerFull.guid,
                                                                    "Em processamento",
                                                                    new List<LogList> {
                                                        new LogList { Title = "Falha ao cadastrar dados do provedor", Message = string.Format("CNPJ empresa: {0} CNPJ provedor:{1}", companyEntrada.companyid, item.Cnpj), Success = false
                                                        } },
                                                                    _bucket,
                                                                    _logger);
                                                            });
                                                        }
                                                        #endregion

                                                        #region tabelaDePreco
                                                        if (item.Products != null)
                                                        {
                                                            foreach (var prod in item.Products)
                                                            {
                                                                if (prod.PriceTable != null)
                                                                {
                                                                    PriceTableOut priceTableOut = new PriceTableOut
                                                                    {
                                                                        hubguid = customerFull.guid,
                                                                        aggregator = aggregador,
                                                                        providerguid = provGuid,
                                                                        providerproductcode = prod.Providerproductcode
                                                                    };
                                                                    priceTableOut.prices = new List<PriceTable>();
                                                                    priceTableOut.prices = prod.PriceTable;

                                                                    if (await PostPriceAsync(priceTableOut, config, authorization))
                                                                    {
                                                                        _ = Task.Run(() =>
                                                                        {
                                                                            operations.UpsertLog(eventToken,
                                                                                customerFull.guid,
                                                                                "Em processamento",
                                                                                new List<LogList> {
                                                        new LogList { Title = "Dados da tabela de preço cadastrado", Message = string.Format("CNPJ empresa: {0} CNPJ provedor:{1}", companyEntrada.companyid, item.Cnpj), Success = true
                                                        } },
                                                                                _bucket,
                                                                                _logger);
                                                                        });
                                                                    }
                                                                    else
                                                                    {
                                                                        _ = Task.Run(() =>
                                                                        {
                                                                            operations.UpsertLog(eventToken,
                                                                                customerFull.guid,
                                                                                "Em processamento",
                                                                                new List<LogList> {
                                                        new LogList { Title = "Falha ao cadastrar dados da tabela de preço", Message = string.Format("CNPJ empresa: {0} CNPJ provedor:{1}", companyEntrada.companyid, item.Cnpj), Success = false
                                                        } },
                                                                                _bucket,
                                                                                _logger);
                                                                        });
                                                                    }
                                                                }
                                                            }

                                                        }
                                                        #endregion
                                                    }
                                                    else
                                                    {
                                                        _ = Task.Run(() =>
                                                        {
                                                            operations.UpsertLog(eventToken,
                                                                customerFull.guid,
                                                                "Em processamento",
                                                                new List<LogList> {
                                                        new LogList { Title = "Provedor não localizado", Message = string.Format("CNPJ empresa: {0} CNPJ provedor:{1}", companyEntrada.companyid, item.Cnpj), Success = false
                                                        } },
                                                                _bucket,
                                                                _logger);
                                                        });
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                _ = Task.Run(() =>
                                                {
                                                    operations.UpsertLog(eventToken,
                                                        customerFull.guid,
                                                        "Em processamento",
                                                        new List<LogList> {
                                                        new LogList { Title = "Nenhum provedor listado", Message = "CNPJ:" + companyEntrada.companyid, Success = true
                                                        } },
                                                        _bucket,
                                                        _logger);
                                                });
                                            }
                                            #endregion

                                            if (companyEntrada.Employees != null)
                                            {
                                                //inicializa a lista de funcionarios
                                                EmployeesOut employeesOut = new EmployeesOut();
                                                employeesOut.hubguid = customerFull.guid;
                                                employeesOut.aggregator = aggregador;
                                                employeesOut.employees = new List<EmployeeInfo>();

                                                //inclui familia dos funcionarios 
                                                foreach (var funcionario in companyEntrada.Employees)
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Dados do funcionario atualizados", Message = string.Format("CNPJ empresa: {0} CPF pessoa:{1}", companyEntrada.companyid, funcionario.Cpf), Success = true
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });

                                                    #region FamilyCompleto
                                                    FamilyCb familyCB = new FamilyCb();
                                                    familyCB.Hubguid = customerFull.guid;
                                                    familyCB.Family = ParseLtBeneficiaryIn(funcionario.Family);
                                                    //familyCB.personguid = personEmployee.guid;
                                                    familyCB.aggregator = aggregador;
                                                    foreach (var dependente in familyCB.Family)
                                                    {
                                                        if (dependente.Typeuser.ToUpper() == "TITULAR")
                                                        {
                                                            dependente.employeeinfo = new FamilyCb.BeneficiaryIn.EmployeeinfoClean
                                                            {
                                                                Registration = funcionario.Registration,
                                                                Admissiondate = funcionario.Admissiondate,
                                                                Occupation = funcionario.Occupation,
                                                                Occupationcode = funcionario.Occupationcode,
                                                                Role = funcionario.Role,
                                                                Rolecode = funcionario.Rolecode,
                                                                Department = funcionario.Department,
                                                                Departmentcode = funcionario.Departmentcode,
                                                                Costcenter = funcionario.Costcenter,
                                                                Costcentercode = funcionario.Costcentercode,
                                                                Union = funcionario.Union,
                                                                Unioncode = funcionario.Unioncode,
                                                                Functionalcategory = funcionario.Functionalcategory,
                                                                Functionalcategorycode = funcionario.Functionalcategorycode,
                                                                Shift = funcionario.Shift,
                                                                Salary = funcionario.Salary,
                                                                Employeecomplementaryinfos = funcionario.Employeecomplementaryinfos
                                                            };
                                                        }
                                                        if (dependente.Benefitinfos != null)
                                                            for (int i = 0; i < dependente.Benefitinfos.Count; i++)
                                                            {
                                                                if (dependente.Benefitinfos[i].providerguid == Guid.Empty)
                                                                {
                                                                    var guidprov = await getProviderAsync(dependente.Benefitinfos[i].providerid, config);
                                                                    if (guidprov.Success)
                                                                        dependente.Benefitinfos[i].providerguid = new Guid(guidprov.Message);
                                                                    else
                                                                    {
                                                                        _ = Task.Run(() =>
                                                                        {
                                                                            operations.UpsertLog(eventToken,
                                                                                customerFull.guid,
                                                                                "Em processamento",
                                                                                new List<LogList> {
                                                        new LogList { Title = "Falha ao tentar localizar provedor do beneficiario", Message = "CNPJ:" + companyEntrada.companyid, Success = false
                                                        } },
                                                                                _bucket,
                                                                                _logger);
                                                                        });
                                                                    }
                                                                }
                                                            }
                                                        dependente.Origin = string.IsNullOrEmpty(dependente.Origin) ? "API" : dependente.Origin;
                                                    }
                                                    #endregion

                                                    #region FamilyPartido
                                                    //FamilyOut familyCB = new FamilyOut();
                                                    //familyCB.Hubguid = customerFull.guid;
                                                    //familyCB.Family = new List<FamilyOut.BeneficiaryOut>();
                                                    //foreach (var dependente in funcionario.Family)
                                                    //{
                                                    //    var personDep = await PostPersonAsync(dependente, config);
                                                    //    if (personDep != null)
                                                    //    {
                                                    //        var dep = (FamilyOut.BeneficiaryOut)dependente;
                                                    //        dep.Origin = string.IsNullOrEmpty(dependente.Origin) ? "API" : dependente.Origin;
                                                    //        dep.Personguid = personDep.guid;
                                                    //        dep.Typeuser = dependente.Typeuser;
                                                    //        dep.Kinship = dependente.Kinship;
                                                    //        dep.Blockdate = dependente.BlockDate;
                                                    //        dep.Blockreason = dependente.BlockReason;
                                                    //        dep.Benefitinfo = dependente.Benefitinfos;
                                                    //        familyCB.Family.Add(dep);
                                                    //    }
                                                    //    else
                                                    //    {
                                                    //        //TODO: Erro de inclusão de pessoa no DB
                                                    //    }
                                                    //}
                                                    #endregion

                                                    var familyInput = PostFamilyAsync(familyCB, config, authorization).Result;
                                                    if (familyInput != null)
                                                    {
                                                        _ = Task.Run(() =>
                                                        {
                                                            operations.UpsertLog(eventToken,
                                                                customerFull.guid,
                                                                "Em processamento",
                                                                new List<LogList> {
                                                        new LogList { Title = "Familia do funcionario atualizada", Message = string.Format("CNPJ empresa: {0} guid funcionario:{1}", companyEntrada.companyid, familyCB.personguid), Success = true
                                                        } },
                                                                _bucket,
                                                                _logger);
                                                        });

                                                        //retirar
                                                        //employeesOut.employees.Add(new EmployeeInfo
                                                        //{
                                                        //    familyguid = familyInput.Guid,
                                                        //    personguid = personEmployee.guid,
                                                        //    Registration = funcionario.Registration,
                                                        //    Admissiondate = funcionario.Admissiondate,
                                                        //    Occupation = funcionario.Occupation,
                                                        //    Occupationcode = funcionario.Occupationcode,
                                                        //    Role = funcionario.Role,
                                                        //    Rolecode = funcionario.Rolecode,
                                                        //    Department = funcionario.Department,
                                                        //    Departmentcode = funcionario.Departmentcode,
                                                        //    Costcenter = funcionario.Costcenter,
                                                        //    Costcentercode = funcionario.Costcentercode,
                                                        //    Union = funcionario.Union,
                                                        //    Unioncode = funcionario.Unioncode,
                                                        //    Functionalcategory = funcionario.Functionalcategory,
                                                        //    Functionalcategorycode = funcionario.Functionalcategorycode,
                                                        //    Shift = funcionario.Shift,
                                                        //    Salary = funcionario.Salary,
                                                        //    Employeecomplementaryinfos = funcionario.Employeecomplementaryinfos
                                                        //});
                                                    }
                                                    else
                                                    {
                                                        _ = Task.Run(() =>
                                                        {
                                                            operations.UpsertLog(eventToken,
                                                                customerFull.guid,
                                                                "Em processamento",
                                                                new List<LogList> {
                                                        new LogList { Title = "Falha ao tentar atualizar familia do funcionario", Message = string.Format("CNPJ empresa: {0} guid funcionario:{1}", companyEntrada.companyid, familyCB.personguid), Success = false
                                                        } },
                                                                _bucket,
                                                                _logger);
                                                        });
                                                    }
                                                }

                                                var incEmployee = await PostEmployeeAsync(employeesOut, config, authorization);
                                                if (incEmployee == null)
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                        new LogList { Title = "Funcionario cadastrado na empresa", Message = string.Format("CNPJ empresa: {0} guid funcionarios:{1}", companyEntrada.companyid, incEmployee.guid), Success = true
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }
                                                else
                                                {
                                                    _ = Task.Run(() =>
                                                    {
                                                        operations.UpsertLog(eventToken,
                                                            customerFull.guid,
                                                            "Em processamento",
                                                            new List<LogList> {
                                                                new LogList { Title = "Falha ao cadastrar funcionario na empresa", Message = string.Format("CNPJ empresa: {0} guid funcionarios:{1}", companyEntrada.companyid, incEmployee.guid), Success = false
                                                        } },
                                                            _bucket,
                                                            _logger);
                                                    });
                                                }

                                            }
                                            else
                                            {
                                                _ = Task.Run(() =>
                                                {
                                                    operations.UpsertLog(eventToken,
                                                        customerFull.guid,
                                                        "Em processamento",
                                                        new List<LogList> {
                                                        new LogList { Title = "Nenhum funcionario listado", Message = "CNPJ:" + companyEntrada.companyid, Success = true
                                                        } },
                                                        _bucket,
                                                        _logger);
                                                });
                                            }

                                            //inclui a empresa na listagem de empresas do orquestrador
                                            hubcontract.Companies.Add(
                                                new HubCustomerOut.CompanyOut
                                                {
                                                    aggregator = aggregador,
                                                    branchName = currentcompanyresult.Branchname,
                                                    Companyguid = currentcompanyresult.guid,
                                                    groupName = objAggreg[0].Group.Name
                                                });

                                        }
                                        else
                                        {
                                            _ = Task.Run(() =>
                                            {
                                                operations.UpsertLog(eventToken,
                                                    customerFull.guid,
                                                    "Em processamento",
                                                    new List<LogList> { new LogList { Title = "Não foi possivel atualizar os dados da empresa", Message = string.Format("ERRO DE SISTEMA - HubCustomer: {0} CNPJ: {1}- Title: {2} - Message: {3}" + customerFull.guid, companyEntrada.companyid, "Erro ao cadastrar unidade", "Erro ao cadastrar unidade."), Success = false } },
                                                    _bucket,
                                                    _logger);
                                            });
                                        }

                                        //envia notificação pra aplicação exibir no front
                                        var modelNotifier = new Notifier();
                                        modelNotifier.hubguid = customerFull.guid.ToString();
                                        modelNotifier.aggregator = aggregador;
                                        modelNotifier.type = "Importação de arquivo";
                                        modelNotifier.title = "Importação de arquivo concluída";
                                        modelNotifier.description = "Importação de arquivo concluída, verifique a area de log para saber mais detalhes";

                                        var cReturn = await Commons.operations.PostNotifierAsync(modelNotifier, config);
                                    }
                                    else
                                    {
                                        _ = Task.Run(() =>
                                          {
                                              operations.UpsertLog(eventToken,
                                                  customerFull.guid,
                                                  "Em processamento",
                                                  new List<LogList> { new LogList { Title = "Dados da empresa/unidade", Message = string.Format("ERRO DE SISTEMA - HubCustomer: {0} CNPJ: {1}- Title: {2} - Message: {3}", customerFull.guid, companyEntrada.companyid, "CNPJ não faz parte da estrutura hierarquica da empresa.", "CNPJ não faz parte da estrutura hierarquica da empresa."), Success = false } },
                                                  _bucket,
                                                  _logger);
                                          });
                                    }
                                }
                            }
                            else
                            {
                                throw new Exception(string.Format("ERRO DE SISTEMA - HubCustomer: {0} - Title: {1} - Message: {2}" + customerFull.guid, "Nenhuma empresa para importar.", "Nenhuma empresa para importar."));
                            }
                            var incHubCustomer = await PostHubCustomerAsync(hubcontract, config);
                        }
                        else
                        {
                            throw new Exception(string.Format("ERRO DE SISTEMA - HubCustomer: {0} - Title: {1} - Message: {2}", customerFull.guid, "Contrato inativo", "Contrato inativo"));
                        }
                    }
                    else
                    {
                        throw new Exception(string.Format("HubCustomer não localizado - HubCustomer: {0} - Title: {1} - Message: {2}", customerFull.guid, "HubCustomer não localizado", "HubCustomer não localizado"));
                    }
                }
                else
                {
                    throw new Exception(string.Format("ERRO DE SISTEMA - HubCustomer: {0} - Title: {1} - Message: {2}", customerFull.guid, "Erro ao tentar localizar HubCustomer", HubContractRequest.Message));
                }
            }
            catch (Exception ex)
            {
                //Atualiza o status do processamento com erro e encerra.
                _ = Task.Run(() =>
                {
                    operations.UpsertLog(eventToken,
                        customerFull.guid,
                        "Finalizado com erro",
                        new List<LogList> {
                            new LogList { Title = "Fim do processamento", Date = DateTime.Now, Success = true, Message = ex.ToString()
                            } },
                        _bucket,
                        _logger);
                });
                throw new Exception(ex.ToString());
            }

            _ = Task.Run(() =>
            {
                operations.UpsertLog(eventToken,
                    customerFull.guid,
                    "Finalizado",
                    new List<LogList> {
                            new LogList { Title = "Fim do processamento", Date = DateTime.Now, Success = true
                            } },
                    _bucket,
                    _logger);
            });
        }

        private static List<FamilyCb.BeneficiaryIn> ParseLtBeneficiaryIn(List<Beneficiary> family)
        {
            List<FamilyCb.BeneficiaryIn> ret = new List<FamilyCb.BeneficiaryIn>();
            foreach (var item in family)
            {
                ret.Add(ParseBeneficiaryIn(item));
            }
            return ret;
        }
        private static FamilyCb.BeneficiaryIn ParseBeneficiaryIn(Beneficiary family)
        {
            var ret = new FamilyCb.BeneficiaryIn
            {
                Addresses = family.Addresses,
                Benefitinfos = family.Benefitinfos,
                Birthdate = family.Birthdate,
                BlockDate = family.BlockDate,
                BlockReason = family.BlockReason,
                Changes = family.Changes,
                complementaryinfos = family.complementaryinfos,
                Cpf = family.Cpf,
                documents = family.documents,
                emailinfos = family.emailinfos,
                Gender = family.Gender,
                Issuingauthority = family.Issuingauthority,
                Kinship = family.Kinship,
                Maritalstatus = family.Maritalstatus,
                Mothername = family.Mothername,
                Name = family.Name,
                Origin = family.Origin,
                Phoneinfos = family.Phoneinfos,
                Rg = family.Rg,
                Sequencial = family.Sequencial,
                Typeuser = family.Typeuser
            };
            return ret;
        }

        #region getMethods
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
        private static async Task<MethodFeedback> getCompanyAsync(string cnpj, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Company") + "CompanyId/{0}", Commons.Helpers.RemoveNaoNumericos(cnpj));
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
        private static async Task<MethodFeedback> getPersonAsync(string cpf, string birthdate, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Person") + "/Cpf/{0}/BirthDate/{1}", Commons.Helpers.RemoveNaoNumericos(cpf), birthdate);
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
        private static async Task<MethodFeedback> getProviderAsync(string cnpj, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Provider") + "/CompanyId/{0}", Commons.Helpers.RemoveNaoNumericos(cnpj));
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
        internal static async Task<IQueryResult<LogCB>> findLogAsync(Guid hubguid, Guid token, IBucket _bucket)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Log' 
and g.guid = $guid
and g.hubguid = $hubguid; ")
                  .AddNamedParameter("$guid", token)
                  .AddNamedParameter("$hubguid", hubguid)
                  .Metrics(false);
            var a = await _bucket.QueryAsync<LogCB>(queryRequest);
            return a;
        }

        #endregion

        #region postMethods
        private static async Task<bool> PostEntityAsync(EntityOut itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Entity") + "/Customer/{0}", itemArq.hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        private static async Task<bool> PostCopartAsync(CopartOut itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Copart") + "/Customer/{0}", itemArq.hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        private static async Task<bool> PostInsuranceClaimAsync(InsuranceclaimOut itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:InsuranceClaim") + "/Customer/{0}", itemArq.hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        private static async Task<MethodFeedback> PostCompanyAsync(CompanyOut itemArq, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Company"));
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
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();

            MethodFeedback ret = new MethodFeedback();
            ret.Success = response.IsSuccessStatusCode;
            ret.Message = apiResponse;

            return ret;
        }
        private static async Task<bool> PostProviderStrucAsync(ProviderCustomerOut itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:ProviderStruc") + "/Customer/{0}", itemArq.hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        private static async Task<MethodFeedback> PostProviderAsync(Provider itemArq, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Provider"));
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
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();

            MethodFeedback ret = new MethodFeedback();
            ret.Success = response.IsSuccessStatusCode;
            ret.Message = apiResponse;

            return ret;
        }
        private static async Task<bool> PostPriceAsync(PriceTableOut itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:PriceTable") + "/Customer/{0}", itemArq.hubguid);
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        private static async Task<PersonOut> PostPersonAsync(Person itemArq, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Person"));
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
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<PersonOut>(apiResponse);
            }

            return null;
        }
        private static async Task<FamilyCb> PostFamilyAsync(FamilyCb itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Family") + "/Customer/{0}", itemArq.Hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<FamilyCb>(apiResponse);
            }

            return null;
        }
        private static async Task<EmployeesOut> PostEmployeeAsync(EmployeesOut itemArq, IConfiguration config, string authorization)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:Employees") + "/Customer/{0}", itemArq.hubguid);
            using var httpClient = new HttpClient();
            HttpClient client = new HttpClient();

            client.BaseAddress = new Uri(Uri);
            client.DefaultRequestHeaders.Accept.Clear();
            client.DefaultRequestHeaders.Add("aggregator", itemArq.aggregator);
            client.DefaultRequestHeaders.Add("Authorization", authorization);
            client.DefaultRequestHeaders.Accept.Add(
                new MediaTypeWithQualityHeaderValue("application/json"));

            var jsonContent = JsonConvert.SerializeObject(itemArq);
            var contentString = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
            contentString.Headers.ContentType = new
            MediaTypeHeaderValue("application/json");
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return JsonConvert.DeserializeObject<EmployeesOut>(apiResponse);
            }

            return null;
        }
        private static async Task<bool> PostHubCustomerAsync(HubCustomerOut itemArq, IConfiguration config)
        {
            string Uri = string.Format(config.GetValue<string>("Endpoints:HubCustomer") + "/{0}", itemArq.Guid);
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
            //contentString.Headers.Add("Session-Token", session_token);

            HttpResponseMessage response = await client.PostAsync(Uri, contentString);
            string apiResponse = await response.Content.ReadAsStringAsync();
            if (response.IsSuccessStatusCode)
            {
                return true;
            }

            return false;
        }
        public static void UpsertLog(Guid token, Guid hubguid, string status, List<LogList> lt, IBucket _bucket, ILogger<OrchestratorController> _logger)
        {
            LogCB log = new LogCB
            {
                guid = token,
                hubguid = hubguid,
                LogList = lt,
                Status = status
            };
            try
            {
                if (log.Status != "Aberto")
                {
                    var ret = _bucket.MutateIn<LogCB>(log.guid.ToString()).Upsert("status", log.Status).Execute();
                    foreach (var logList in log.LogList)
                    {
                        logList.Date = DateTime.Now;
                        _logger.LogInformation("Operation: {0} - Status: {1} - Title: {2} - Message: {3}", log.guid, log.Status, logList.Title, logList.Message);

                        var updateAddress = _bucket.MutateIn<LogCB>(log.guid.ToString()).
                            ArrayAppend("logList", logList).
                            Execute();
                    }
                }
                else
                {
                    var a = _bucket.Upsert(
                    log.guid.ToString(), new
                    {
                        log.guid,
                        log.hubguid,
                        docType = "Log",
                        location = "Orchestrator",
                        log.LogList,
                        start = DateTime.Now,
                        log.Status
                    });
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
            }
        }
        #endregion
    }
}