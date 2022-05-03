using Commons;
using CompanyAppservice.Models;
using CompanyAppService.Repository.Interface;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CompanyAppService.Repository
{
    
    public class CompanyRepository : ICompanyRepository
    {
        private readonly IBucket _bucket;
        public CompanyRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }
        public async Task<List<CompanyCB>> GetCompanyByCNPJAsync(string CNPJ)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Company' 
and g.companyid = $Cnpj;")
                  .AddNamedParameter("$Cnpj", CNPJ)
                  .Metrics(false);
            var a = await _bucket.QueryAsync<CompanyCB>(queryRequest);
            if (!a.Success || a.Rows.Count == 0)
                return null;
            return a.Rows;
        }

        public async Task<List<CompanyCB>> GetCompanyByGUIDAsync(Guid token)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Company' 
and g.guid = $guid;")
                  .AddNamedParameter("$guid", token)
                  .Metrics(false);
            var a = await _bucket.QueryAsync<CompanyCB>(queryRequest);
            if (!a.Success || a.Rows.Count == 0)
                return null;
            return a.Rows;
        }

        public async Task<CompanyCB> PostAsync(CompanyCB company)
        {
            //se guid estiver vazio, busca na base com outros parametros
            var search = await GetCompanyByCNPJAsync(company.companyid);
            if (search != null)
            {
                var find = search.SingleOrDefault();
                company.guid = find.guid;
                company.companyid = find.companyid;
                company.Branchname = find.Branchname;
                company.Companyname = find.Companyname;
                company.TradingName = find.TradingName;
                company.Description = find.Description;

                if (company.Addresses != null)
                    _bucket.MutateIn<dynamic>(company.guid.ToString()).Upsert("addresses", company.Addresses).Execute();

                if (company.Complementaryinfos != null)
                {
                    List<Complementaryinfo> complementaryinfos = new List<Complementaryinfo>();
                    complementaryinfos = find.Complementaryinfos;
                    foreach (var ci in company.Complementaryinfos)
                    {
                        int index = find.Complementaryinfos.FindIndex(x => x.type == ci.type);
                        if (index < 0)
                        {//inclui empresa
                            complementaryinfos.Add(ci);
                        }
                        else
                        {
                            complementaryinfos[index] = ci;
                        }
                    }
                    _bucket.MutateIn<dynamic>(company.guid.ToString()).Upsert("complementaryinfos", complementaryinfos).Execute();

                }

            }
            //se não achar, atribui novo guid para cadastro novo
            else
            {
                company.guid = Guid.NewGuid();
                var a = _bucket.Upsert(
                company.guid.ToString(), new
                {
                    company.guid,
                    docType = "Company",
                    companyid = Commons.Helpers.RemoveNaoNumericos(company.companyid),
                    company.Branchname,
                    company.Companyname,
                    company.TradingName,
                    company.Description,
                    company.Addresses,
                    company.Complementaryinfos
                });
            }


            return company;
        }
    }
}
