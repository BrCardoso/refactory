using IdentityService.Models;
using IdentityService.Services.Interfaces;

using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace IdentityService.Services
{
	public class TokenService : ITokenService
	{
		private readonly IConfiguration _configuration;

		public TokenService(IConfiguration configuration) =>
			_configuration = configuration;

		public TokenModel Generate(UserModel user)
		{
			Guid accessToken = Guid.NewGuid();
			Guid refreshToken = Guid.NewGuid();
			DateTime now = DateTime.Now;
			DateTime expiresIn = now.AddHours(1);

			string token = new JwtSecurityTokenHandler().WriteToken(new JwtSecurityToken(
							claims: FillClaims(user),
							expires: expiresIn,
							signingCredentials: new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])), SecurityAlgorithms.HmacSha256
							)));

			if (string.IsNullOrEmpty(token))
				return null;

			return new TokenModel
			{
				UserGuid = user.Guid,
				AccessToken = accessToken,
				RefreshToken = refreshToken,
				CreatedDateTime = now,
				ExpiresDateTime = expiresIn,
				Token = token
			};
		}

		private IEnumerable<Claim> FillClaims(UserModel user)
		{
			return new List<Claim>()
			{
				new Claim("sub", user.UserName.Replace("HUB/", "")),
				new Claim("accessLevel", user.Permissions.AccessLevel),
				new Claim("makeInvoiceConference", user.Permissions.MakeInvoiceConference.ToString()),
				new Claim("accessToBI", user.Permissions.AccessToBI.ToString()),
				new Claim("cnpj", user.Permissions.Cnpj),
				new Claim("given_name", user.Name.GivenName),
				new Claim("makeLoadRequest", user.Permissions.MakeLoadRequest.ToString()),
				new Claim("userid", user.Guid.ToString()),
				new Claim("changeOtherUsers", user.Permissions.ChangeOtherUsers.ToString()),
				new Claim("userType", user.Permissions.UserType.ToString()),
				new Claim("family_name", user.Name.FamilyName),
				new Claim("approveNewLogins", user.Permissions.ApproveNewLogins.ToString()),
				new Claim("status", user.Permissions.Status.ToString())
			};
		}
	}
}