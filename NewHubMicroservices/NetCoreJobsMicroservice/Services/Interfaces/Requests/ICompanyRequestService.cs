using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Services.Interfaces.Requests
{
    public interface ICompanyRequestService
    {
        Task<CompanyDB> getCompanyAsync(string cnpj);
    }
}
