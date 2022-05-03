using Commons.Base;
using InputAppService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InputAppService.Repository.Interface
{
    public interface IEntityRepository
    {
        Task<EntityModel> GetAsync(Guid hubguid, string aggregator);
        Task<EntityModel> GetAsync(Guid hubguid, string aggregator, Guid provguid);
        bool Mutate(Guid guid, List<Entity> entities);
        bool Upsert(EntityModel model);
    }
}
