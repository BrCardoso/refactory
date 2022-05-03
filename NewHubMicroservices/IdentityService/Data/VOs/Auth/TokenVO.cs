using System;

namespace IdentityService.Data.VOs.Auth
{
	public class TokenVO
	{
		public Guid AccessToken { get; set; }
		public Guid RefreshToken { get; set; }
		public string Token { get; set; }
		public ushort ExpiresIn { get; set; }
	}
}