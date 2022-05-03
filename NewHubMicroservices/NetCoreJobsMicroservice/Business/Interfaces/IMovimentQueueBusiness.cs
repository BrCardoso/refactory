using Commons;
using NetCoreJobsMicroservice.Models;
using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Business.Interfaces
{
    public interface IMovimentQueueBusiness
    {
        Task<MethodFeedback> HandleFileAsync(Guid token, QueueModel model, string authorization);
    }
}
