using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BIAppService.Model;
using BIAppService.ServiceRequest.Interface;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace BIAppService.Controllers
{
    [Route("api/v1/[controller]")]
    public class BIController : Controller
    {
        private readonly IServiceRequests _serviceRequests;
        private readonly IBucket _bucket;

        public BIController(IServiceRequests serviceRequests, IBucketProvider bucket)
        {
            _serviceRequests = serviceRequests;
            _bucket = bucket.GetBucket("DataBucket001");
        }

        [HttpGet("getModel")]
        public async Task<IActionResult> GetGeneralAsync()
        {
            List<VisaoGeral2> result = new List<VisaoGeral2> { new VisaoGeral2 { beneficiarios = new List<VisaoGeral2.Beneficiarios> { new VisaoGeral2.Beneficiarios { beneficios = new List<VisaoGeral2.Beneficiarios.Beneficio> { new VisaoGeral2.Beneficiarios.Beneficio() } } } } };

            return Ok(result);

        }
        [HttpGet]
        [Route("{token}")]
        public async Task<object> GetAsync(Guid token)
        {
            try
            {
                if (token == Guid.Empty)
                {
                    return BadRequest("Missing token.");
                }

                var queryRequest = new QueryRequest()
                        .Statement(@"SELECT count(d)funcionarios
,(SELECT COUNT(ddd.family) FROM DataBucket001 ddd
   UNNEST ddd.family as fff
   where ddd.docType= 'Family'
   and ddd.hubguid = $guidhub
   and ARRAY_COUNT(ddd.family) > 1
   and fff.typeuser != 'Titular')[0].`$1` dependentes
,(SELECT pp.birthdate, pp.gender, ff.typeuser, pp.addresses[0].state, pp.addresses[0].city,dd.aggregator
   FROM DataBucket001 as dd
   UNNEST dd.family as ff JOIN DataBucket001 pp ON KEYS ff.personguid
   UNNEST ff.benefitinfos bb
   where dd.docType = 'Family'
   and dd.hubguid = '374f4ad4-d577-4f13-a20b-23c84c6a7990') detalhes
,(SELECT hub.hierarchy.`groups` FROM DataBucket001 as hub where hub.guid = $guidhub)[0].`groups` hubstruc
FROM DataBucket001 as d
UNNEST d.family as f JOIN DataBucket001 p ON KEYS f.personguid
UNNEST f.benefitinfos b
where d.docType = 'Family'
and d.hubguid = $guidhub")
                        .AddNamedParameter("$guidhub", token)
                        .Metrics(false);

                var result = await _bucket.QueryAsync<dynamic>(queryRequest);
                if (result.Success)
                {
                    return Ok(JsonConvert.SerializeObject(result.Rows));
                }
                else
                {
                    return Problem(result.Message);
                }
            }
            catch (Exception ex)
            {
                return Problem(ex.ToString());
            }
        }

        [HttpGet("Customer/{token}")]
        public async Task<object> GetGeneralAsync(Guid token, string Authorization)
        {
            List<VisaoGeral> result = new List<VisaoGeral>();

            var HubCustomer = _serviceRequests.GetHubCustomerAsync(token, Authorization);
            var Beneficiaries = _serviceRequests.GetBeneficiariesAsync(token, Authorization);
            await Task.WhenAll(HubCustomer, Beneficiaries);

            if (Beneficiaries.Result?.Count > 0)
            {
                //PREENCHE OS DADOS DO FAMILY
                for (int idxFamily = 0; idxFamily < Beneficiaries.Result?.Count; idxFamily++)
                {
                    for (int idxPerson = 0; idxPerson < Beneficiaries.Result[idxFamily].family?.Count; idxPerson++)
                    {
                        for (int idxBenef = 0; idxBenef < Beneficiaries.Result[idxFamily].family[idxPerson].Benefitinfos?.Count; idxBenef++)
                        {
                            var pessoa = Beneficiaries.Result[idxFamily].family[idxPerson];
                            var beneficio = Beneficiaries.Result[idxFamily].family[idxPerson].Benefitinfos[idxBenef];

                            result.Add(new VisaoGeral
                            {
                                beneficiario_cidade = pessoa.Addresses?[0].city,
                                beneficiario_cpf = pessoa.Cpf,
                                beneficiario_idade = Idade(pessoa.Birthdate),
                                beneficiario_nome = pessoa.Name,
                                beneficiario_id = pessoa.personguid.ToString(),
                                beneficiario_sexo = pessoa.Gender,
                                beneficiario_uf = pessoa.Addresses?[0].state,
                                beneficiario_tipo = Beneficiaries.Result[idxFamily].family[idxPerson].Typeuser,

                                beneficio_bloqueio_data = beneficio.blockdate,
                                beneficio_bloqueio_motivo = beneficio.BlockReason,
                                beneficio_contrato = beneficio.contractnumber,
                                //beneficio_incDate = ,
                                //beneficio_preco = ,
                                beneficio_produtocode = beneficio.productcode,
                                beneficio_provedorguid = beneficio.providerguid,

                                codgrupo = Beneficiaries.Result[idxFamily].aggregator.Split("-")[0],
                                codempresa = Beneficiaries.Result[idxFamily].aggregator.Split("-")[1],
                                cnpj = Beneficiaries.Result[idxFamily].aggregator.Split("-")[2]
                            });
                        }
                    }
                }

                //ORGANIZA POR PROVEDOR
                result = result.OrderBy(x => x.beneficio_provedorguid).ThenBy(y => y.beneficio_produtocode).ToList();

                //PREENCHE OS DADOS DO PROVEDOR
                var provider = await _serviceRequests.GetProviderAsync(result[0].beneficio_provedorguid, Authorization);
                var provedorNome = provider.Description;
                var provedorSeg = provider.Segcode;

                var produto = provider.Products.Where(p => p.Providerproductcode == result[0].beneficio_produtocode).SingleOrDefault();
                var provedorProd = produto.Description;
                var provedorProdSeg = produto.Subsegcode;

                for (int i = 0; i < result.Count; i++)
                {
                    //mudou de provedor
                    if (provider.guid != result[i].beneficio_provedorguid)
                    {
                        provider = await _serviceRequests.GetProviderAsync(result[i].beneficio_provedorguid, Authorization);
                        provedorNome = provider.Description;
                        provedorSeg = provider.Segcode;
                        produto = provider.Products.Where(p => p.Providerproductcode == result[i].beneficio_produtocode).SingleOrDefault();
                        provedorProd = produto.Description;
                        provedorProdSeg = produto.Subsegcode;
                    }
                    //mudou de produto
                    if (produto.Providerproductcode != result[i].beneficio_produtocode)
                    {
                        produto = provider.Products.Where(p => p.Providerproductcode == result[i].beneficio_produtocode).SingleOrDefault();
                        provedorProd = produto.Description;
                        provedorProdSeg = produto.Subsegcode;
                    }
                    result[i].beneficio_provedor = provedorNome;
                    result[i].segcode = provedorSeg;
                    result[i].beneficio_produto = provedorProd;
                    result[i].subsegcode = provedorProdSeg;
                }

                //ORGANIZA POR AGGREGATOR
                result = result.OrderBy(x => x.codgrupo + x.codempresa + x.cnpj).ToList();
                var aggregator = result[0].codgrupo + result[0].codempresa + result[0].cnpj;

                var idxGrupo = HubCustomer.Result.hierarchy.Groups.FindIndex(x => x.Code == result[0].codgrupo);
                var idxEmpresa = HubCustomer.Result.hierarchy.Groups[idxGrupo].Companies.FindIndex(x => x.Code == result[0].codempresa);

                var filialDetails = await _serviceRequests.GetCompanyAsync(result[0].cnpj, Authorization);

                //PREENCHE OS DADOS DO GRUPO EMPRESA
                for (int i = 0; i < result.Count; i++)
                {
                    //mudou de grupo
                    if (HubCustomer.Result.hierarchy.Groups[idxGrupo].Code != result[i].codgrupo)
                    {
                        idxGrupo = HubCustomer.Result.hierarchy.Groups.FindIndex(x => x.Code == result[i].codgrupo);
                        idxEmpresa = HubCustomer.Result.hierarchy.Groups[idxGrupo].Companies.FindIndex(x => x.Code == result[i].codempresa);
                    }
                    //mudou de empresa
                    if (HubCustomer.Result.hierarchy.Groups[idxGrupo].Companies[idxEmpresa].Code != result[i].codgrupo)
                    {
                        idxEmpresa = HubCustomer.Result.hierarchy.Groups[idxGrupo].Companies.FindIndex(x => x.Code == result[i].codempresa);
                    }
                    //mudou de filial
                    if (filialDetails.companyid != result[i].cnpj)
                    {
                        filialDetails = await _serviceRequests.GetCompanyAsync(result[i].cnpj, Authorization);
                    }

                    result[i].grupo = HubCustomer.Result.hierarchy.Groups[idxGrupo].Name;
                    result[i].empresa = HubCustomer.Result.hierarchy.Groups[idxGrupo].Companies[idxEmpresa].Name;
                    result[i].unidade = filialDetails.Branchname;
                }

                //RETORNA O OBJETO
                return Ok(result);
            }

            return BadRequest(result);
        }

        private string Idade(DateTime birthdate)
        {
            // Save today's date.
            var today = DateTime.Today;

            // Calculate the age.
            var age = today.Year - birthdate.Year;

            // Go back to the year the person was born in case of a leap year
            if (birthdate.Date > today.AddYears(-age)) age--;

            return age.ToString();
        }
    }
}
