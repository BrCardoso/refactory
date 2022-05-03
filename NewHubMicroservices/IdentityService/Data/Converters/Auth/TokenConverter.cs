using Commons.Enums;

using IdentityService.Data.Converters.Interfaces;
using IdentityService.Data.VOs.Auth;
using IdentityService.Exceptions;
using IdentityService.Models;

using System.Net;

namespace IdentityService.Data.Converters.Auth
{
	public class TokenConverter : IParser<(TokenModel token, ushort expiresIn)?, TokenVO>
	{
		public TokenVO Parse((TokenModel token, ushort expiresIn)? origin)
		{
			return origin switch
			{
				null => throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.API_ORIGIN_IN_CONVERTER_IS_NULL),
				_ => new TokenVO
				{
					AccessToken = origin.Value.token.AccessToken,
					RefreshToken = origin.Value.token.RefreshToken,
					Token = origin.Value.token.Token,
					ExpiresIn = origin.Value.expiresIn
				}
			};
		}
	}
}