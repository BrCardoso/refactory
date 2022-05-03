using CompanyAppservice.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CompanyAppService.Repository.Interface
{
    public interface ICompanyRepository
    {
        Task<List<CompanyCB>> GetCompanyByCNPJAsync(string CNPJ);
        Task<List<CompanyCB>> GetCompanyByGUIDAsync(Guid token);
        Task<CompanyCB> PostAsync(CompanyCB company);
    }
}
