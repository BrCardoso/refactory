using IdentityService.Models;

using System.Threading.Tasks;

namespace IdentityService.Repository.Interfaces
{
	public interface IIdentityTokensRepository
	{
		Task<IdentityTokensModel> FindAsync();

		Task<IdentityTokensModel> UpSertAsync(IdentityTokensModel identityTokens);
	}
}