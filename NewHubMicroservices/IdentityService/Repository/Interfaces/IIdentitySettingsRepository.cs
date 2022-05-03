using IdentityService.Models;

using System.Threading.Tasks;

namespace IdentityService.Repository.Interfaces
{
	public interface IIdentitySettingsRepository
	{
		Task<IdentitySettingsModel> FindAsync();
	}
}