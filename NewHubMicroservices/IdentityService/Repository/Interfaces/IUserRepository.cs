using IdentityService.Models;

using System.Threading.Tasks;

namespace IdentityService.Repository.Interfaces
{
	public interface IUserRepository
	{
		Task<IdentityModel> FindAsync();

		Task<IdentityModel> UpSertAsync(IdentityModel identity);
	}
}