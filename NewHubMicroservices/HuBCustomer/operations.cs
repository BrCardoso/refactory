using Couchbase.Core;
using Couchbase.N1QL;
using System;
using System.Threading.Tasks;
using HuBCustomerAppService.Models;
using System.Collections.Generic;
using Commons.Base;
using System.Linq;
using Couchbase;

namespace HuBCustomerAppService
{
    public class operations {
        public static async Task<HuBCustomerModel> UpsertAsync(HuBCustomerModel contract, string aggregator, IBucket _bucket)
        {
            try
            {
                //se guid estiver vazio, busca na base com outros parametros
                var find = await getContractAsync(contract.guid, _bucket);
                if (find.Success && find.Rows.Count == 1) 
                {
                    if (contract.hierarchy != null)
                    {
                        _bucket.MutateIn<dynamic>(contract.guid.ToString()).Upsert("hierarchy",  contract.hierarchy ).Execute();
                    }

                    if (contract.companies != null)
                    {
                        List<CompanyStruCB> companies = new List<CompanyStruCB>();
                        companies = find.Rows[0].companies;
                        if (companies == null)
                        {
                            companies = new List<CompanyStruCB>();
                        }
                        foreach (var company in contract.companies)
                        {
                            int index = find.Rows[0].companies.FindIndex(x => x.companyguid == company.companyguid);
                            if (index < 0)
                            {
                                //inclui empresa
                                companies.Add(company);
                            }
                            else
                            {
                                companies[index].aggregator = company.aggregator != null ? company.aggregator : find.Rows[0].companies[index].aggregator;
                                companies[index].branchName = company.branchName != null ? company.branchName : find.Rows[0].companies[index].branchName;
                                companies[index].companyguid = company.companyguid;
                                companies[index].GroupName = company.GroupName != null ? company.GroupName : find.Rows[0].companies[index].GroupName;
                            }
                        }
                        _bucket.MutateIn<dynamic>(contract.guid.ToString()).Upsert("companies", companies).Execute();
                    }                    
                }
                return null;
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                throw;
            }
        }

        internal static async Task<IQueryResult<EmployeesModel>> getEmployeeByGuidAsync(Guid hubguid, string aggregator, Guid personguid, IBucket bucket)
        {
            var query = new QueryRequest()
                       .Statement(@"SELECT g.* FROM DataBucket001 g 
unnest g.employees as e
    where g.docType = 'Employees' 
    and g.hubguid = $hubguid
    and g.aggregator = $aggregator
    and e.personguid = $personguid
    and e.registration is not null
; ")
                   .AddNamedParameter("$hubguid", hubguid)
                   .AddNamedParameter("$aggregator", aggregator)
                   .AddNamedParameter("$personguid", personguid)
                   .Metrics(false);

            return await bucket.QueryAsync<EmployeesModel>(query);
        }

        internal static async Task<onboarding> UpsertOnBoardingAsync(onboarding model, IBucket _bucket)
        {
            try
            {
                var find = await getOnBoardingAsync(model, _bucket);
                if (find.Success && find.Rows.Count == 1)
                {
                    if (model.steps != null)
                    {
                        model.steps.provider = model.steps.provider == false ? find.Rows[0].steps.provider : model.steps.provider;
                        model.steps.benefit = model.steps.benefit == false ? find.Rows[0].steps.benefit : model.steps.benefit;
                        model.steps.pricetable = model.steps.pricetable == false ? find.Rows[0].steps.pricetable : model.steps.pricetable;
                        model.steps.entity = model.steps.entity == false ? find.Rows[0].steps.entity : model.steps.entity;
                        model.steps.rulesconfiguration = model.steps.rulesconfiguration == false ? find.Rows[0].steps.rulesconfiguration : model.steps.rulesconfiguration;
                        model.steps.beneficiary = model.steps.beneficiary == false ? find.Rows[0].steps.beneficiary : model.steps.beneficiary;

                        var ret = _bucket.MutateIn<dynamic>(find.Rows[0].guid.ToString()).Upsert("steps", model.steps).Execute();

                        if (model.steps.provider == true &&
                            model.steps.benefit == true &&
                            model.steps.pricetable == true &&
                            model.steps.entity == true &&
                            model.steps.rulesconfiguration == true &&
                            model.steps.beneficiary == true
                            )
                        {
                            model.status = true;
                            ret = _bucket.MutateIn<dynamic>(find.Rows[0].guid.ToString()).Upsert("status", model.status).Execute();
                        }
                    }
                }
                else
                {
                    model.guid = Guid.NewGuid();
                    var a = _bucket.Upsert(
                    model.guid.ToString(), new
                    {
                        model.guid,
                        model.hubguid,
                        model.aggregator,
                        docType = "OnBoarding",
                        model.status,
                        model.steps
                    });

                }
                return model;
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                throw;
            }
        }
        internal static async Task<EmployeesModel> UpsertEmployeeAsync(EmployeesModel model, IBucket _bucket)
        {
            try
            {
                var find = await getEmployeeAsync(model, _bucket);
                if (find.Success && find.Rows.Count == 1)
                {

                    if (model.employees != null)
                    {
                        List<EmployeeInfo> employees = new List<EmployeeInfo>();
                        employees = find.Rows[0].employees;
                        foreach (var employee in model.employees)
                        {
                            int index = find.Rows[0].employees.FindIndex(x => x.Registration == employee.Registration && x.personguid == employee.personguid);
                            if (index < 0)
                            {//inclui produto
                                employees.Add(employee);
                            }
                            else
                            {
                                employees[index] = employee;
                            }
                        }
                        var ret = _bucket.MutateIn<dynamic>(find.Rows[0].guid.ToString()).Upsert("employees", employees).Execute();
                    }
                }
                else
                {
                    model.guid = Guid.NewGuid();
                    var a = _bucket.Upsert(
                    model.guid.ToString(), new
                    {
                        model.guid,
                        model.hubguid,
                        docType = "Employees",
                        model.aggregator,
                        model.employees
                    });

                }
                return model;
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                throw;
            }
        }
        internal static async Task<IOperationResult<HRResponsableModel.opResponsable>> UpsertHRResponsableAsync(HRResponsableModel.opResponsable addResponsable, IBucket _bucket)
        {
            if (addResponsable.guid == Guid.Empty)
                addResponsable.guid = Guid.NewGuid();
            addResponsable.docType = "HRResponsable";
            var result = await UpdateHRResponsableAsync(addResponsable, _bucket);
            return result;
        }

        public static async Task<IQueryResult<HuBCustomerModel>> getContractAsync(Guid token, IBucket _bucket)
        {
            var query = new QueryRequest()
                   .Statement(@"SELECT g.* FROM DataBucket001 g where g.docType = 'HubCustomer' and g.guid = $guid;")
                   .AddNamedParameter("$guid", token)
                   .Metrics(false);
            return await _bucket.QueryAsync<HuBCustomerModel>(query);
        }
        public static async Task<IQueryResult<HuBCustomerModel>> getContractAsync(string companyid, IBucket _bucket)
        {
            var query = new QueryRequest()
                   .Statement(@"SELECT g.* FROM DataBucket001 g where g.docType = 'HubCustomer' and g.contractIssued = $companyid;")
                   .AddNamedParameter("$companyid", companyid)
                   .Metrics(false);
            return await _bucket.QueryAsync<HuBCustomerModel>(query);
        }
        internal static async Task<IQueryResult<EmployeesModel>> getEmployeeAsync(EmployeesModel model, IBucket _bucket)
        {
            var query = new QueryRequest()
                      .Statement(@"SELECT g.* FROM DataBucket001 g 
    where g.docType = 'Employees' 
    and g.hubguid = $hubguid
    and g.aggregator = $aggregator; ")
                  .AddNamedParameter("$hubguid", model.hubguid)
                  .AddNamedParameter("$aggregator", model.aggregator)
                  .Metrics(false);
            if (model.aggregator == null)
            {
                query = new QueryRequest()
                     .Statement(@"SELECT g.* FROM DataBucket001 g 
where g.docType = 'Employees' 
and g.hubguid = $hubguid; ")
                     .AddNamedParameter("$hubguid", model.hubguid)
                     .Metrics(false);

            }
            return await _bucket.QueryAsync<EmployeesModel>(query);
        }
        internal static async Task<IQueryResult<EmployeesModel>> getEmployeeByRegistrationAsync(Guid hubguid, string aggregator, string registration, IBucket _bucket)
        {
            var query = new QueryRequest()
                      .Statement(@"SELECT g.* FROM DataBucket001 g 
unnest g.employees as e
    where g.docType = 'Employees' 
    and g.hubguid = $hubguid
    and g.aggregator = $aggregator
    and e.registration = $registration
    and e.registration is not null
; ")
                  .AddNamedParameter("$hubguid", hubguid)
                  .AddNamedParameter("$aggregator", aggregator)
                  .AddNamedParameter("$registration", registration)
                  .Metrics(false);
            
            return await _bucket.QueryAsync<EmployeesModel>(query);
        }
        internal static async Task<IQueryResult<onboarding>> getOnBoardingAsync(onboarding model, IBucket _bucket)
        {
            var query = new QueryRequest()
                      .Statement(@"SELECT g.* FROM DataBucket001 g 
    where g.docType = 'OnBoarding' 
    and g.hubguid = $hubguid
    and g.aggregator = $aggregator; ")
                  .AddNamedParameter("$hubguid", model.hubguid)
                  .AddNamedParameter("$aggregator", model.aggregator)
                  .Metrics(false);
            
            return await _bucket.QueryAsync<onboarding>(query);
        }

        public static async Task<IEnumerable<HRResponsableModel.Responsable>> FindAllAsync(string aggregator, Guid hubGuid, IBucket _bucket)
        {
            var query = new QueryRequest()
                   .Statement(@"SELECT d.*
							FROM DataBucket001 d
							WHERE d.docType='HRResponsable'
								AND d.hubguid = $hubguid
								AND d.aggregator = $aggregator;")
                   .AddNamedParameter("$hubguid", hubGuid)
                   .AddNamedParameter("$aggregator", aggregator)
                   .Metrics(false);

            var result = await _bucket.QueryAsync<HRResponsableModel.Responsable>(query);
            return result.Select(x => x);
        }

        public static async Task<IEnumerable<HRResponsableModel.Responsable>> FindAllBySessionAsync(string aggregator, Guid hubGuid, string session, IBucket _bucket)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT d.*
							FROM DataBucket001 d
							WHERE ARRAY_CONTAINS(d.sessions, $session)
								AND d.docType='HRResponsable'
								AND d.hubguid = $hubguid
								AND d.aggregator = $aggregator;")
                .AddNamedParameter("$session", session)
                .AddNamedParameter("$hubguid", hubGuid)
                .AddNamedParameter("$aggregator", aggregator)
                .Metrics(false);

            var result = await _bucket.QueryAsync<HRResponsableModel.Responsable>(query);
            return result.Select(x => x);
        }

        public static async Task<HRResponsableModel.Responsable> FindByGuidAsync(Guid guid, IBucket _bucket)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT g.* FROM DataBucket001 as g WHERE g.docType='HRResponsable' AND g.guid = $guid;")
                .AddNamedParameter("$guid", guid).
                Metrics(false);

            var result = await _bucket.QueryAsync<HRResponsableModel.Responsable>(query);

            return result.SingleOrDefault(x => x.guid == guid);
        }

        public static async Task<HRResponsableModel.Responsable> FindByCPFAsync(string cpf, IBucket _bucket)
        {
            var query = new QueryRequest()
                .Statement(@"SELECT g.* FROM DataBucket001 as g WHERE g.docType='HRResponsable' AND g.cpf = $cpf;")
                .AddNamedParameter("$cpf", cpf).
                Metrics(false);

            var result = await _bucket.QueryAsync<HRResponsableModel.Responsable>(query);

            return result.SingleOrDefault(x => x.cpf == cpf);
        }

        internal static async Task<IOperationResult<HRResponsableModel.opResponsable>> UpdateHRResponsableAsync(HRResponsableModel.opResponsable newValues, IBucket _bucket)
        {
            newValues.docType = "HRResponsable";
            return await _bucket.UpsertAsync(newValues.guid.ToString(), newValues);
        }
    }
}
