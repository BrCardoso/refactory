using Commons;
using NetCoreJobsMicroservice.Models;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface INITRequestService
    {
        Task<MethodFeedback> PostNITAsync(Commons.Base.Nit.NitModel itemArq);
    }
}
