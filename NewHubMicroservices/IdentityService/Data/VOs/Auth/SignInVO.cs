using System.ComponentModel.DataAnnotations;

namespace IdentityService.Data.VOs.Auth
{
	public class SignInVO
	{
		[Required]
		public string Email { get; set; }

		[Required]
		[StringLength(30, MinimumLength = 6)]
		public string Password { get; set; }
	}
}