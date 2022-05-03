using NetCoreJobsMicroservice.Models;
using System;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository.Interface
{
    public interface ITaskPanelRepository
    {
        Task<bool> AddTaskPanelAsync(TaskPanel taskPanel);
        TaskPanel findTask(Guid hubguid, Guid docid, Guid personguid, string movType);
    }
}
