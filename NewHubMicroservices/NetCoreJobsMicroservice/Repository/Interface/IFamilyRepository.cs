using NetCoreJobsMicroservice.Models;
using System.Threading.Tasks;

namespace NetCoreJobsMicroservice.Repository.Interface
{
    public interface IFamilyRepository
	{
		Task<FamilyDB> UpdatePersonCardNumberAsync(FamilyDB family);
	}
}