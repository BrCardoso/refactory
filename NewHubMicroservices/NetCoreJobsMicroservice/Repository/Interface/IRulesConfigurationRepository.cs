using NetCoreJobsMicroservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository.Interface
{
    public interface IRulesConfigurationRepository
    {
        Task<List<RulesConfigurationModel>> getRCByContractsAsync(Guid token, Guid provider, string contract);
        string findRC(Guid hubguid, Guid? docid, Guid providerguid, string contractnumber);
        RulesConfigurationModel getRC(Guid hubguid, Guid? docid, Guid providerguid, string contractnumber);
    }
}
