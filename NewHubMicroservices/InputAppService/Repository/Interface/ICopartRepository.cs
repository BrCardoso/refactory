using InputAppService.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InputAppService.Repository.Interface
{
    public interface ICopartRepository
    {
        Task<List<CopartModel>> Get(Guid token, string aggregator);
        Task<CopartModel> Get(Guid token, string aggregator, Guid provider);
        bool Mutate(Guid guid, List<Coparticipation> Coparticipations);
        bool Upsert(CopartModel model);
    }
}
