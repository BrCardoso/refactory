using Commons;

using LoginAppService.Converters;
using LoginAppService.Data.VOs.User;
using LoginAppService.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace LoginAppService.Controllers
{
	[Route("api/v1/[controller]")]
	[ApiController]
	public class UserController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IConfigPasswordService _configPasswordService;

		public UserController(IUserService userService, IConfigPasswordService configPasswordService)
		{
			_userService = userService;
			_configPasswordService = configPasswordService;
		}

		[HttpPost("Tree")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task<IActionResult> CreateSingleAsync([FromBody] CreateUserVO newSingleUser)
		{
			if (ModelState.IsValid)
			{
				if (!newSingleUser.UserName.StartsWith("HUB/"))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USERNAME_INVALID", Message = "Inicie o UserName com \'HUB/\'", Success = false });

				if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
					return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

				string approveNewLogins = claims.FirstOrDefault(x => x.Type == "approveNewLogins").Value;
				if (approveNewLogins.ToUpper() != "TRUE")
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_HAVE_PERM_TO_ADD", Success = false });

				if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND_OR_PROBLEM", Success = false });

				if (users.Any(x => x.UserName == newSingleUser.UserName))
					return BadRequest(new MethodFeedback { MessageCode = "USER_ALREADY_EXISTS_USERNAME", Success = false });

				newSingleUser.Password = Guid.NewGuid().ToString().Substring(0, 30);

				if (!(await _userService.CreateUserDataAsync(newSingleUser) is UserVO addedUser))
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_CREATED", Success = false });

				await _configPasswordService.RequestAsync(new Model.ConfigPassword.Data
				{
					requestDateTime = DateTime.Now,
					requestguid = Guid.NewGuid(),
					userguid = addedUser.Id
				}, newSingleUser.UserName.Replace("HUB/", ""));

				return Ok(addedUser);
			}

			return BadRequest();
		}

		[HttpGet]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task<IActionResult> GetDataAsync()
		{
			if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
				return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

			string email = claims.FirstOrDefault(x => x.Type == "sub").Value;

			if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
				return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND_OR_PROBLEM", Success = false });

			UserVO user = users.FirstOrDefault(x => x.Emails.Contains(email));
			if (!(user is UserVO))
				return BadRequest(new MethodFeedback { MessageCode = "USERS_USER_NOT_FOUND", Success = false });

			return Ok(user);
		}

		[HttpGet("Tree")]
		[ProducesResponseType(typeof(IEnumerable<UserVO>), 200)]
		public async Task<IActionResult> GetAllSinglesAsync()
		{
			if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
				return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

			if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
				return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND_OR_PROBLEM", Success = false });

			var usersFiltered = GetUsersFromAccessLevel(users, claims);

			return Ok(usersFiltered);
		}

		[HttpPatch("Block/{guid?}")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task<IActionResult> BlockUserAsync([Required] Guid? guid)
		{
			if (ModelState.IsValid)
			{
				if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
					return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

				if (claims.FirstOrDefault(x => x.Type == "changeOtherUsers").Value?.ToUpper() != "TRUE")
					return BadRequest(new MethodFeedback { MessageCode = "USER_DONT_HAVE_PERM_TO_BLOCK", Success = false });

				if (!(await _userService.BlockUserAsync(guid.Value) is UserVO userBlocked))
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_BLOCKED", Success = false });

				return Ok(userBlocked);
			}

			return BadRequest();
		}

		[HttpPatch("Unblock/{guid?}")]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task<IActionResult> UnblockUserAsync([Required] Guid? guid)
		{
			if (ModelState.IsValid)
			{
				if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
					return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

				if (claims.FirstOrDefault(x => x.Type == "changeOtherUsers").Value?.ToUpper() != "TRUE")
					return BadRequest(new MethodFeedback { MessageCode = "USER_DONT_HAVE_PERM_TO_UNBLOCK", Success = false });

				if (!(await _userService.UnblockUserAsync(guid.Value) is UserVO userBlocked))
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_UNBLOCKED", Success = false });

				return Ok(userBlocked);
			}

			return BadRequest();
		}

		[HttpPut]
		[ProducesResponseType(typeof(UserVO), 200)]
		public async Task<IActionResult> UpdateDataAsync([FromBody] UpdateLoggedUserVO newUserData)
		{
			if (ModelState.IsValid)
			{
				if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
					return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

				string userId = claims.FirstOrDefault(x => x.Type == "userid").Value;
				string email = claims.FirstOrDefault(x => x.Type == "sub").Value;

				if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND_OR_PROBLEM", Success = false });

				UserVO user = users.FirstOrDefault(x => x.Emails.Contains(email));
				if (!(user is UserVO))
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_FOUND", Success = false });

				if (!(await _userService.UpdateUserDataAsync(UpdateUserConverter.Parse(user, newUserData)) is UserVO updatedUser))
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_UPDATED", Success = false });

				return Ok(updatedUser);
			}

			return BadRequest();
		}

		[HttpPut("Tree")]
		public async Task<IActionResult> UpdateSingleDataAsync([FromBody] UpdateUserVO newSingleUserData)
		{
			if (ModelState.IsValid)
			{
				if (!newSingleUserData.UserName.StartsWith("HUB/"))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USERNAME_INVALID", Message = "Inicie o UserName com \'HUB/\'", Success = false });

				if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
					return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

				if (claims.FirstOrDefault(x => x.Type == "changeOtherUsers").Value?.ToUpper() != "TRUE")
					return BadRequest(new MethodFeedback { MessageCode = "USER_DONT_HAVE_PERM_TO_UPDATE", Success = false });

				if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND_OR_PROBLEM", Success = false });

				var usersFiltered = GetUsersFromAccessLevel(users, claims);

				UserVO user = usersFiltered.FirstOrDefault(x => x.Id == newSingleUserData.Id);
				if (!(user is UserVO))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USER_NOT_FOUND_OR_NOT_PERM", Success = false });

				if (!user.Permissions.AccessLevel.StartsWith(claims.FirstOrDefault(x => x.Type == "accessLevel").Value))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USER_CANNOT_CREATE_LANGER_ACCESSLEVEL", Success = false });

				if (!(await _userService.UpdateUserDataAsync(newSingleUserData) is UserVO updatedUser))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USER_NOT_UPDATED", Success = false });

				return Ok(updatedUser);
			}

			return BadRequest();
		}

		[HttpDelete("{guid?}")]
		[ProducesResponseType(204)]
		public async Task<IActionResult> DeleteUserAsync([Required] Guid? guid)
		{
			if (ModelState.IsValid)
			{
				if (!(GetUserClaimsFromToken() is IEnumerable<Claim> claims))
					return BadRequest(new MethodFeedback { MessageCode = "INVALID_TOKEN", Success = false });

				if (claims.FirstOrDefault(x => x.Type == "changeOtherUsers").Value?.ToUpper() != "TRUE")
					return BadRequest(new MethodFeedback { MessageCode = "USER_DONT_HAVE_PERM_TO_DELETE", Success = false });

				if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND_OR_PROBLEM", Success = false });

				var usersFiltered = GetUsersFromAccessLevel(users, claims);

				UserVO user = usersFiltered.FirstOrDefault(x => x.Id == guid.Value);
				if (!(user is UserVO))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USER_NOT_FOUND_OR_NOT_PERM", Success = false });

				if (!await _userService.DeleteUserAsync(guid.ToString()))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_USER_NOT_DELETED", Success = false });

				return NoContent();
			}

			return BadRequest();
		}

		private IEnumerable<Claim> GetUserClaimsFromToken()
		{
			try
			{
				string token = Request.Headers["Authorization"].ToString().Replace("Bearer ", "");

				var handlerToken = new JwtSecurityTokenHandler();

				var jwt = handlerToken.ReadToken(token) as JwtSecurityToken;

				return jwt.Claims;
			}
			catch
			{ return null; }
		}

		private IEnumerable<UserVO> GetUsersFromAccessLevel(IEnumerable<UserVO> users, IEnumerable<Claim> claims)
		{
			string cnpj = claims.FirstOrDefault(x => x.Type == "cnpj").Value;
			string accessLevel = claims.FirstOrDefault(x => x.Type == "accessLevel").Value;

			var usersByCNPJ = users.Where(x => x.Permissions?.Cnpj == cnpj);

			var usersFilteredByNoMaster = usersByCNPJ.Where(x => x.Permissions.UserType.ToString().ToUpper() != "MASTER");

			var usersFilteredByAccessLevel = usersFilteredByNoMaster.Where(x => x.Permissions?.AccessLevel?.StartsWith(accessLevel) == true);

			return usersFilteredByAccessLevel;
		}
	}
}