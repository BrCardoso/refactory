using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using BeneficiaryAppService.Models;
using BeneficiaryAppService.Models.External;
using Couchbase;
using Couchbase.N1QL;

namespace BeneficiaryAppService.Repository.Interfaces
{
    public interface IPersonRepository
    {

        Task<PersonDB> FindByPersonGuidAsync(Guid guid);
        Task<PersonDB> FindByPersonNameAsync(string name, string cpf, DateTime? birth);
        Task<IOperationResult> AddAsync(PersonDB newDocument);
        Task<bool> Update(PersonDB document);
        bool UpSertChanges(Guid docid, List<Commons.Change> changes);
        Task<bool>UpSertDocumentsAsync(Guid docid, List<Commons.Document> documents);
        Task<bool>UpSertComplInfosAsync(Guid docid, List<Commons.Complementaryinfo> complementaryinfos);
        IQueryResult<object> getField(Guid docid, string mandatoryrulesField);
    }
}
