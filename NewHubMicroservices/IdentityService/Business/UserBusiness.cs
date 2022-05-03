using Commons.Enums;

using IdentityService.Business.Interfaces;
using IdentityService.Data.Converters.User;
using IdentityService.Data.VOs.User;
using IdentityService.Exceptions;
using IdentityService.Models;
using IdentityService.Repository.Interfaces;
using IdentityService.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IdentityService.Business
{
	public class UserBusiness : IUserBusiness
	{
		private readonly IUserRepository _userRepository;
		private readonly UserConverter _userConverter;
		private readonly CreateUserConverter _createUserConverter;
		private readonly UpdateUserConverter _updateUserConverter;
		private readonly ISecurityService _securityService;

		public UserBusiness(IUserRepository userRepository, ISecurityService securityService, UserConverter userConverter, CreateUserConverter createUserConverter, UpdateUserConverter updateUserConverter)
		{
			_userRepository = userRepository;
			_userConverter = userConverter;
			_createUserConverter = createUserConverter;
			_updateUserConverter = updateUserConverter;
			_securityService = securityService;
		}

		public async Task CreateUserAsync(CreateUserVO newUserData)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();
			identityUsers ??= new IdentityModel { DocType = "Identity", Guid = Guid.NewGuid(), Users = new List<UserModel> { } };

			if (identityUsers?.Users?.Any(x => x.UserName.ToUpper() == newUserData.UserName.ToUpper()) == true)
				throw new HTTPException(HttpStatusCode.Conflict, MessageCode.USER_EMAIL_ALREADY_EXISTS);

			UserModel userModel = _createUserConverter.Parse(newUserData);

			userModel.Guid = Guid.NewGuid();
			userModel.Meta = new UserModel.MetaModel { Created = DateTime.Now, LastModified = DateTime.Now };

			userModel.Password = _securityService.EncryptToSHA256(newUserData.Password);

			identityUsers.Users.Add(userModel);

			if (!(await _userRepository.UpSertAsync(identityUsers) is IdentityModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.USER_NOT_CREATED);

			throw new ResponseHTTPException(HttpStatusCode.Created, MessageCode.USER_SUCCESSFULLY_CREATED, _userConverter.Parse(userModel));
		}

		public async Task FindByUserGuidAsync(Guid userGuid)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			if (!(identityUsers?.Users?.SingleOrDefault(x => x.Guid == userGuid) is UserModel userCurrent))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.USER_NOT_FOUND);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.USER_FOUND, _userConverter.Parse(userCurrent));
		}

		public async Task FindAllUsersAsync()
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			IEnumerable<UserModel> users = identityUsers?.Users;
			if (!(users is IEnumerable<UserModel>) || users?.Count() == 0)
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.USERS_NOT_FOUND);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.USERS_FOUND, _userConverter.Parse(users));
		}

		public async Task BlockUserAsync(Guid userGuid)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			if (!(identityUsers?.Users?.SingleOrDefault(x => x.Guid == userGuid) is UserModel userCurrent))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.USER_NOT_FOUND);

			if (userCurrent.Permissions.Status == UserStatus.Bloqueado)
				throw new HTTPException(HttpStatusCode.Conflict, MessageCode.USER_ALREADY_BLOCKED);

			userCurrent.Permissions.Status = UserStatus.Bloqueado;
			userCurrent.Meta = new UserModel.MetaModel { Created = userCurrent.Meta.Created, LastModified = DateTime.Now };

			if (!(await _userRepository.UpSertAsync(identityUsers) is IdentityModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.USER_NOT_BLOCKED);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.USER_SUCCESSFULLY_BLOCKED, _userConverter.Parse(userCurrent));
		}

		public async Task UnblockUserAsync(Guid userGuid)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			if (!(identityUsers?.Users?.SingleOrDefault(x => x.Guid == userGuid) is UserModel userCurrent))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.USER_NOT_FOUND);

			if (userCurrent.Permissions.Status == UserStatus.Ativo)
				throw new HTTPException(HttpStatusCode.Conflict, MessageCode.USER_NOT_BLOCKED);

			userCurrent.Permissions.Status = UserStatus.Ativo;
			userCurrent.Meta = new UserModel.MetaModel { Created = userCurrent.Meta.Created, LastModified = DateTime.Now };

			if (!(await _userRepository.UpSertAsync(identityUsers) is IdentityModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.USER_NOT_BLOCKED);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.USER_SUCCESSFULLY_UNBLOCKED, _userConverter.Parse(userCurrent));
		}

		public async Task UpdateUserData(UpdateUserVO newUserData)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			if (!(identityUsers?.Users?.SingleOrDefault(x => x.Guid == newUserData.Id) is UserModel userCurrent))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.USER_NOT_FOUND);

			var updatedData = _updateUserConverter.Parse(newUserData);

			updatedData.Password = string.IsNullOrEmpty(newUserData.Password) ? userCurrent.Password : _securityService.EncryptToSHA256(newUserData.Password);
			updatedData.Meta = new UserModel.MetaModel { Created = userCurrent.Meta.Created, LastModified = DateTime.Now };

			identityUsers.Users.Remove(userCurrent);
			identityUsers.Users.Add(updatedData);

			if (!(await _userRepository.UpSertAsync(identityUsers) is IdentityModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.USER_NOT_UPDATED);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.USER_SUCCESSFULLY_UPDATED, _userConverter.Parse(updatedData));
		}

		public async Task DeleteUserAsync(Guid userGuid)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			if (!(identityUsers?.Users?.SingleOrDefault(x => x.Guid == userGuid) is UserModel userCurrent))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.USER_NOT_FOUND);

			identityUsers.Users.Remove(userCurrent);

			if (!(await _userRepository.UpSertAsync(identityUsers) is IdentityModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.USER_NOT_DELETED);

			throw new ResponseHTTPException(HttpStatusCode.NoContent, MessageCode.USER_SUCCESSFULLY_DELETED, null);
		}
	}
}