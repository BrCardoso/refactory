using LoginAppService.Data.VOs.User;

using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace LoginAppService.Services.Interfaces
{
	public interface IUserService
	{
		Task<UserVO> CreateUserDataAsync(CreateUserVO newUserData);

		Task<List<UserVO>> GetUsersInfoAsync();

		Task<UserVO> BlockUserAsync(Guid userguid);

		Task<UserVO> UnblockUserAsync(Guid userguid);

		Task<UserVO> UpdateUserDataAsync(UpdateUserVO newUserData);

		Task<bool> DeleteUserAsync(string id);
	}
}