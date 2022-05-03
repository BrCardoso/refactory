using NetCoreJobsMicroservice.Models;
using System;
using System.Collections.Generic;

namespace NetCoreJobsMicroservice.Repository.Interface
{
    public interface IMandatoryRulesRepository
    {
        MandatoryRules Get(Guid guid);
        List<MandatoryRules> GetAll();
        List<MandatoryRules> GetByProvider(Guid providerguid);
        List<MandatoryRules> GetByProvider(Guid providerguid, string movType);
        List<MandatoryRules> GetByProvider(Guid providerguid, string movType, string kinship);
        List<MandatoryRules> GetBySegment(string segment);
        bool Upsert(MandatoryRules model);
    } 
}
