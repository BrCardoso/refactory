using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Couchbase.Core;
using Couchbase.Extensions.DependencyInjection;
using Couchbase.N1QL;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Repository.Interfaces;
using Couchbase;
using Commons;

namespace BeneficiaryAppService.Repository
{
    public class PersonRepository : IPersonRepository
    {
        private readonly IBucket _bucket;

        public PersonRepository(IBucketProvider bucketProvider)
        {
            _bucket = bucketProvider.GetBucket("DataBucket001");
        }
        public async Task<IOperationResult> AddAsync(PersonDB newDocument)
        {
            if (newDocument.Guid == Guid.Empty)
                newDocument.Guid = Guid.NewGuid();
            return await UpSert(newDocument);
        }
        private async Task<IOperationResult> UpSert(PersonDB document)
        {
            var resultDB = await _bucket.UpsertAsync(document.Guid.ToString(), new
            {
                document.Guid,
                docType = "Person",
                document.Name,
                document.Gender,
                document.Cpf,
                birthdate = document.Birthdate.ToString("yyyy-MM-dd"),
                document.Rg,
                document.Issuingauthority,
                document.Mothername,
                document.Maritalstatus,
                document.Addresses,
                document.Phoneinfos,
                document.emailinfos,
                document.Changes,
                document.complementaryinfos,
                document.expeditiondate,
                document.documents
            });

            return resultDB;
        }

        public async Task<bool> Update(PersonDB document)
        {
            _ = _bucket.MutateIn<dynamic>(document.Guid.ToString()).Upsert("expeditiondate", document.expeditiondate).Execute();
            _ = _bucket.MutateIn<dynamic>(document.Guid.ToString()).Upsert("emailinfos", document.emailinfos).Execute();
            _ = _bucket.MutateIn<dynamic>(document.Guid.ToString()).Upsert("phoneinfos", document.Phoneinfos).Execute();

            return true;
        }
        public bool UpSertChanges(Guid docid, List<Change> changes)
        {
            try
            {
                foreach (var item in changes)
                {
                    var result = _bucket.MutateIn<PersonDB>(docid.ToString()).
                        ArrayAppend("changes", item).
                        Execute();
                    if (result.Success == false)
                        return false;
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UpSertDocumentsAsync(Guid docid, List<Commons.Document> documents)
        {
            try
            {
                var find = await FindByPersonGuidAsync(docid);
                if (find != null)
                {
                    //instancia uma nova lista de documentos caso nao exista
                    if (find.documents == null)
                        find.documents = new List<Commons.Document>();

                    if (documents != null)
                    {
                        foreach (var doc in documents)
                        {
                            int index = find.documents.FindIndex(x => x.type == doc.type);
                            if (index < 0)
                            {
                                //inclui empresa
                                find.documents.Add(doc);
                            }
                            else
                            {
                                find.documents[index] = doc;
                            }
                        }
                        var exe = _bucket.MutateIn<dynamic>(docid.ToString()).Upsert("documents", find.documents).Execute();
                        if (exe.Success)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<bool> UpSertComplInfosAsync(Guid docid, List<Commons.Complementaryinfo> complementaryinfos)
        {
            try
            {
                var find = await FindByPersonGuidAsync(docid);
                if (find != null)
                {//instancia uma nova lista de complementaryinfos caso nao exista
                    if (find.complementaryinfos == null)
                        find.complementaryinfos = new List<Commons.Complementaryinfo>();

                    if (complementaryinfos != null)
                    {
                        foreach (var ci in complementaryinfos)
                        {
                            int index = find.complementaryinfos.FindIndex(x => x.type == ci.type);
                            if (index < 0)
                            {
                                //inclui empresa
                                find.complementaryinfos.Add(ci);
                            }
                            else
                            {
                                find.complementaryinfos[index] = ci;
                            }
                        }
                        var exe = _bucket.MutateIn<dynamic>(docid.ToString()).Upsert("complementaryinfos", find.complementaryinfos).Execute();
                        if (exe.Success)
                            return true;
                    }
                }
                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }
        public async Task<PersonDB> FindByPersonGuidAsync(Guid guid)
        {
            var query = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g WHERE g.docType = 'Person' and g.guid = $guid;")
                  .AddNamedParameter("$guid", guid)
                  .Metrics(false);

            var result = await _bucket.QueryAsync<PersonDB>(query);
            return result.SingleOrDefault();
        }
        public async Task<PersonDB> FindByPersonNameAsync(string name, string cpf, DateTime? birth)
        {
            //procura por nome, data de nascimento e cpf
            var query = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Person' 
and g.cpf = $cpf 
and g.birthdate = $birth 
and UPPER(g.name) = $name;")
                  .AddNamedParameter("$name", name?.Trim().ToUpper())
                  .AddNamedParameter("$cpf", cpf)
                  .AddNamedParameter("$birth", birth?.ToString("yyyy-MM-dd"))
                  .Metrics(false);

            var result = await _bucket.QueryAsync<PersonDB>(query);

            if (result.Rows.Count == 1)
                return result.SingleOrDefault();
            else//procura por nome e cpf
            {
                query = new QueryRequest()
                  .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE     g.docType = 'Person' and g.cpf = $cpf and UPPER(g.name) = $name;")
                  .AddNamedParameter("$name", name?.Trim().ToUpper())
                  .AddNamedParameter("$cpf", cpf)
                  .Metrics(false);

                result = await _bucket.QueryAsync<PersonDB>(query);
                if (result.Rows.Count == 1)
                    return result.SingleOrDefault();
                else//procura por nome, data de nascimento
                {
                    query = new QueryRequest()
                      .Statement(@"SELECT g.* FROM DataBucket001 g 
WHERE g.docType = 'Person' and g.birthdate = $birth and UPPER(g.name) = $name ;")
                      .AddNamedParameter("$name", name?.Trim().ToUpper())
                      .AddNamedParameter("$birth", birth?.ToString("yyyy-MM-dd"))
                      .Metrics(false);

                    result = await _bucket.QueryAsync<PersonDB>(query);
                    if (result.Rows.Count == 1)
                        return result.SingleOrDefault();
                    else
                        return null;
                }
            }
        }
        public IQueryResult<object> getField(Guid docid, string mandatoryrulesField)
        {
            var query = new QueryRequest()
                                        .Statement(string.Format(@"SELECT {0} FROM DataBucket001 g where g.docType = 'Person' AND g.guid = $guid;"
        , mandatoryrulesField.ToLower()))
                                        .AddNamedParameter("$guid", docid)
                                        .Metrics(false);

            return _bucket.Query<object>(query);
        }

    }
}

class Result
{
    public Guid id { get; set; }
}