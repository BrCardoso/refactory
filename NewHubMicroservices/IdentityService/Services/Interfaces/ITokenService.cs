using IdentityService.Models;

namespace IdentityService.Services.Interfaces
{
	public interface ITokenService
	{
		TokenModel Generate(UserModel user);
	}
}