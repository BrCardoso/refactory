using LoginAppService.Model;

using System.Threading.Tasks;

namespace LoginAppService.Services.Interfaces
{
	public interface IConfigPasswordService
	{
		Task RequestAsync(ConfigPassword.Data configData, string email);

		Task<bool> ChangePasswordAsync(string token, string newPassword);
	}
}