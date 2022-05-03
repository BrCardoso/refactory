using System.ComponentModel.DataAnnotations;

namespace LoginAppService.Models
{
	public class ConfigPasswordInput
	{
		public class Request
		{
			[Required]
			public string Email { get; set; }
		}

		public class Change
		{
			[Required]
			public string NewPassword { get; set; }

			[Required]
			public string ConfirmationNewPassword { get; set; }

			[Required]
			public string Token { get; set; }
		}
	}
}