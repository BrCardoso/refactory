using Commons.Enums;

using IdentityService.Business.Interfaces;
using IdentityService.Data.Converters.Auth;
using IdentityService.Data.VOs.Auth;
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
	public class AuthBusiness : IAuthBusiness
	{
		private readonly IUserRepository _userRepository;
		private readonly IIdentityTokensRepository _identityTokensRepository;
		private readonly ITokenService _tokenService;
		private readonly ISecurityService _securityService;
		private readonly TokenConverter _tokenConverter;

		public AuthBusiness(IUserRepository userRepository, IIdentityTokensRepository identityTokensRepository, ITokenService tokenService, ISecurityService securityService, TokenConverter tokenConverter)
		{
			_userRepository = userRepository;
			_identityTokensRepository = identityTokensRepository;
			_tokenService = tokenService;
			_securityService = securityService;
			_tokenConverter = tokenConverter;
		}

		public async Task SignInAsync(SignInVO signIn)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();

			UserModel user = identityUsers?.Users?.SingleOrDefault(x => x.UserName.Replace("HUB/", "") == signIn.Email && x.Password == _securityService.EncryptToSHA256(signIn.Password));

			if (user is null)
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.AUTH_USER_NOT_FOUND);

			IdentityTokensModel identityTokens = await _identityTokensRepository.FindAsync();
			if (identityTokens is null)
				identityTokens = new IdentityTokensModel
				{
					Guid = Guid.NewGuid(),
					Tokens = new List<TokenModel> { }
				};

			if (user.Permissions.Status == UserStatus.Bloqueado)
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.USER_BLOCKED);

			//TODO: isso retorna o token que ja existe no banco, mas nao sei se é correto fazer isso
			//DateTime now = DateTime.Now;
			//if (identityTokens?.Tokens?.SingleOrDefault(x => x.UserGuid == user.Guid && DateTime.Compare(x.ExpiresDateTime, now) > 0) is TokenModel currentToken)
			//{
			//	if (DateTime.Compare(currentToken.ExpiresDateTime, now) > 0)
			//		throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.AUTH_SUCCESSFULLY, _tokenConverter.Parse((currentToken, (ushort)(currentToken.ExpiresDateTime - now).TotalSeconds)));
			//}

			TokenModel token = _tokenService.Generate(user);
			if (!(token is TokenModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.AUTH_TOKEN_NOT_GENERATED);

			identityTokens.Tokens.Add(token);
			await _identityTokensRepository.UpSertAsync(identityTokens);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.AUTH_SUCCESSFULLY, _tokenConverter.Parse((token, (ushort)(token.ExpiresDateTime
				- DateTime.Now).TotalSeconds)));
		}

		public async Task ValidateToken(Guid accessToken)
		{
			IdentityTokensModel identityTokens = await _identityTokensRepository.FindAsync();

			if (identityTokens?.Tokens?.SingleOrDefault(x => x.AccessToken == accessToken) is TokenModel currentToken)
			{
				DateTime now = DateTime.Now;
				if (DateTime.Compare(currentToken.ExpiresDateTime, now) > 0)
					throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.AUTH_SUCCESSFULLY, _tokenConverter.Parse((currentToken, (ushort)(currentToken.ExpiresDateTime - now).TotalSeconds)));

				throw new HTTPException(HttpStatusCode.Forbidden, MessageCode.AUTH_ACCESS_NOT_IS_VALID);
			}

			throw new HTTPException(HttpStatusCode.NotFound, MessageCode.AUTH_ACCESS_NOT_FOUND);
		}

		public async Task RefreshTokenAsycn(Guid refreshToken)
		{
			IdentityModel identityUsers = await _userRepository.FindAsync();
			IdentityTokensModel identityTokens = await _identityTokensRepository.FindAsync();

			if (!(identityTokens?.Tokens?.SingleOrDefault(x => x.RefreshToken == refreshToken) is TokenModel currentToken))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.AUTH_REFRESH_TOKEN_NOT_FOUND);

			if (!(identityUsers.Users.SingleOrDefault(x => x.Guid == currentToken.UserGuid) is UserModel))
				throw new HTTPException(HttpStatusCode.NotFound, MessageCode.AUTH_REFRESH_TOKEN_USER_INVALID);

			currentToken.RefreshToken = Guid.NewGuid();
			DateTime now = DateTime.Now;
			currentToken.CreatedDateTime = now;
			currentToken.ExpiresDateTime = now.AddHours(1);

			if (!(await _identityTokensRepository.UpSertAsync(identityTokens) is IdentityTokensModel))
				throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.AUTH_REFRESH_TOKEN_NOT_REFRESHTED);

			throw new ResponseHTTPException(HttpStatusCode.OK, MessageCode.AUTH_REFRESH_TOKEN_REFRESHTED, _tokenConverter.Parse((currentToken, (ushort)(currentToken.ExpiresDateTime
				- DateTime.Now).TotalSeconds)));
		}
	}
}