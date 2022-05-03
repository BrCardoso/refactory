using LoginAppService.Model;

using System.Threading.Tasks;

namespace LoginAppService.Repository.Interfaces
{
	public interface IConfigPasswordRepository
	{
		Task<ConfigPassword> FindAsync();

		Task<bool> UpSertAsync(ConfigPassword configPassword);
	}
}