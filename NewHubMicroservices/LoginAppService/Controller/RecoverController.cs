using Commons;

using LoginAppService.Data.VOs.User;
using LoginAppService.Models;
using LoginAppService.Services.Interfaces;

using Microsoft.AspNetCore.Mvc;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LoginAppService.Controllers
{
	[Route("api/v1/[controller]")]
	[ApiController]
	public class RecoverController : ControllerBase
	{
		private readonly IUserService _userService;
		private readonly IConfigPasswordService _configPasswordService;

		public RecoverController(IUserService userService, IConfigPasswordService configPasswordService)
		{
			_userService = userService;
			_configPasswordService = configPasswordService;
		}

		[HttpGet]
		public object Get()
		{
			var ret = new ConfigPasswordInput.Request();
			return Ok(ret);
		}

		[HttpPost]
		public async Task<IActionResult> RequestRecoverAsync(ConfigPasswordInput.Request userData)
		{
			if (ModelState.IsValid)
			{
				if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
					return BadRequest(new MethodFeedback { MessageCode = "USERS_NOT_FOUND", Success = false });

				var user = users.SingleOrDefault(x => x.UserName.ToUpper() == $"HUB/{userData.Email.ToUpper()}");
				if (user is null)
					return BadRequest(new MethodFeedback { MessageCode = "USER_NOT_FOUND", Success = false });

				await _configPasswordService.RequestAsync(new Model.ConfigPassword.Data
				{
					requestDateTime = DateTime.Now,
					requestguid = Guid.NewGuid(),
					userguid = user.Id
				}, userData.Email);

				return Ok(new MethodFeedback { MessageCode = "USER_CONFIG_PASSWORD_SUCCESSFULLY_SENT" });
			}

			return BadRequest();
		}

		[HttpPut("change")]
		public async Task<IActionResult> ChangePasswordAsync([FromBody] ConfigPasswordInput.Change newPassword)
		{
			if (ModelState.IsValid)
			{
				if (newPassword.NewPassword != newPassword.ConfirmationNewPassword)
					return BadRequest(new MethodFeedback { MessageCode = "USER_PASSWORDS_DONT_MATCH", Success = false });

				if (await _configPasswordService.ChangePasswordAsync(newPassword.Token, newPassword.NewPassword))
					return Ok(new MethodFeedback { MessageCode = "USER_SUCCESSFULLY_CHANGE_PASSWORD" });

				return BadRequest(new MethodFeedback { MessageCode = "USER_PROBLEM_TO_UPDATE_PASSWORD", Success = false });
			}

			return BadRequest();
		}
	}
}