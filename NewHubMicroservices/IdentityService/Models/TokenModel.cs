using System;

namespace IdentityService.Models
{
	public class TokenModel
	{
		public Guid AccessToken { get; set; }
		public Guid RefreshToken { get; set; }
		public Guid UserGuid { get; set; }
		public string Token { get; set; }
		public DateTime CreatedDateTime { get; set; }
		public DateTime ExpiresDateTime { get; set; }
		//public DateTime? RefreshDateTime { get; set; }
	}
}