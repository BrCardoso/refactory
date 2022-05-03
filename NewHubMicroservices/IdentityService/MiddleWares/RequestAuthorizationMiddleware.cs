using IdentityService.Models;
using IdentityService.Repository.Interfaces;
using IdentityService.Services.Interfaces;

using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Extensions;

using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace IdentityService.MiddleWares
{
	public class RequestAuthorizationMiddleware
	{
		private readonly RequestDelegate _next;

		public RequestAuthorizationMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public async Task InvokeAsync(HttpContext context, IIdentitySettingsRepository identitySettingsRepository, ISecurityService securityService)
		{
			if (!context.Request.GetDisplayUrl().ToLower().Contains("api"))
			{
				await _next(context);
				return;
			}

			string basic = context.Request.Headers["Authorization"];
			if (string.IsNullOrEmpty(basic) || basic?.StartsWith("Basic") == false)
			{
				context.Response.StatusCode = 401;
				return;
			}

			string base64 = basic.Replace("Basic ", "");

			if (!IsBase64String(base64))
			{
				context.Response.StatusCode = 401;
				return;
			}

			var bytes = Convert.FromBase64String(base64);
			var loginsDecoded = Encoding.UTF8.GetString(bytes);

			string[] login = loginsDecoded.Split(':');
			if (login.Length != 2)
			{
				context.Response.StatusCode = 401;
				return;
			}

			IdentitySettingsModel identitySettings = await identitySettingsRepository.FindAsync();

			if (identitySettings?.Settings?.Users?.SingleOrDefault(x => x.Email == login[0] && x.Password == securityService.EncryptToSHA256(login[1])) == null)
			{
				context.Response.StatusCode = 401;
				return;
			}

			await _next(context);
		}

		private static bool IsBase64String(string s)
		{
			s = s.Trim();
			return (s.Length % 4 == 0) && Regex.IsMatch(s, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None);
		}
	}
}