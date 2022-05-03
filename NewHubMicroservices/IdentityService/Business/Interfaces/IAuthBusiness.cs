using IdentityService.Data.VOs.Auth;

using System;
using System.Threading.Tasks;

namespace IdentityService.Business.Interfaces
{
	public interface IAuthBusiness
	{
		Task SignInAsync(SignInVO signIn);

		Task ValidateToken(Guid accessToken);

		Task RefreshTokenAsycn(Guid refreshToken);
	}
}