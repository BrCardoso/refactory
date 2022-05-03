using Commons;
using Commons.Base;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using ProvidersAppservice.Models;
using ProvidersAppservice.Repository.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProvidersAppservice.Repository
{
    public class ProviderRepository : IProviderRepository
    {
        private readonly IBucket _bucket;
        public ProviderRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }

        public async Task<ProviderCB> FindByCNPJ(string cnpj)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Provider' and  g.cnpj = $Cnpj;")
                  .AddNamedParameter("$Cnpj", cnpj)
                  .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public async Task<ProviderCB> FindByName(string name)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Provider' and g.description = $Description;")
                  .AddNamedParameter("$Description", name?.ToUpper().Trim())
                  .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCB>(queryRequest);
            return a.SingleOrDefault();
        }

        public async Task<ProviderCB> FindByRegistration(string registration)
        {
            var queryRequest = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
where g.docType = 'Provider' and g.registration = $Registration;")
                  .AddNamedParameter("$Registration", registration)
                  .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCB>(queryRequest);
            return a.FirstOrDefault();
        }

        public async Task<List<ProviderCB>> Get()
        {
            var query = new QueryRequest()
                    .Statement(@"SELECT g.* FROM DataBucket001 g where g.docType = 'Provider';")
                    .Metrics(false);
            var a =  await _bucket.QueryAsync<ProviderCB>(query);
            return a.Rows;
        }
        public async Task<ProviderCB> Get(Guid token)
        {
            var query = new QueryRequest()
                    .Statement(@"SELECT g.* FROM DataBucket001 g where g.docType = 'Provider' and g.guid = $guid;")
                    .AddNamedParameter("$guid", token)
                    .Metrics(false);
            var a = await _bucket.QueryAsync<ProviderCB>(query);
            return a.SingleOrDefault();
        }

        public async Task<ProviderCB> Upsert(ProviderCB provider)
        {
            //limpa caracteres não numericos do cnpj se houuver
            provider.Cnpj = Helpers.RemoveNaoNumericos(provider.Cnpj);
            try
            {
                //se guid estiver vazio, busca na base com outros parametros
                var find = await FindByCNPJ(provider.Cnpj);

                //Busca pela descricao
                if (find == null)
                    find = await FindByName(provider.Description);

                //Busca pelo codigo da ANS (caso seja fornecido)
                if (find == null && !string.IsNullOrEmpty(provider.Registration))
                    find = await FindByRegistration(provider.Registration);

                if (find != null)
                {
                    provider.guid = find.guid;
                    provider.Cnpj = find.Cnpj;
                    provider.Description = find.Description;
                    provider.Registration = find.Registration;
                    provider.Segcode = find.Segcode;
                    provider.Site = find.Site;
                    provider.Status = find.Site;

                    if (provider.Products != null)
                    {
                        List<Product> products = new List<Product>();
                        products = find.Products;
                        foreach (var currProd in provider.Products)
                        {
                            int index = find.Products.FindIndex(x => x.Providerproductcode == currProd.Providerproductcode);
                            if (index < 0)
                            {//inclui produto
                                products.Add(currProd);
                            }
                        }
                        _bucket.MutateIn<dynamic>(provider.guid.ToString()).Upsert("products", products).Execute();

                    }
                }
                //se não achar, atribui novo guid para cadastro novo
                else
                {
                    provider.guid = Guid.NewGuid(); provider.Status = "Ativo";
                    var a = _bucket.Upsert(
                       provider.guid.ToString(), new
                       {
                           provider.guid,
                           docType = "Provider",
                           cnpj = provider.Cnpj,
                           Description = provider.Description.ToUpper().Trim(),
                           provider.Products,
                           registration = Commons.Helpers.RemoveNaoNumericos(provider.Registration),
                           provider.Segcode,
                           provider.Site,
                           provider.Status
                       });
                }
            }
            catch (Exception ex)
            {
                _ = ex.ToString();
                throw;
            }
            return provider;
        }
    }
}
