using IdentityService.Data.VOs.User;

using System;
using System.Threading.Tasks;

namespace IdentityService.Business.Interfaces
{
	public interface IUserBusiness
	{
		Task CreateUserAsync(CreateUserVO newUserData);

		Task FindByUserGuidAsync(Guid userGuid);

		Task FindAllUsersAsync();

		Task BlockUserAsync(Guid userGuid);

		Task UnblockUserAsync(Guid userGuid);

		Task UpdateUserData(UpdateUserVO newUserData);

		Task DeleteUserAsync(Guid userGuid);
	}
}