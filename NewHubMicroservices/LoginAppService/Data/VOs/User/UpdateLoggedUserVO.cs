using System.ComponentModel.DataAnnotations;

namespace LoginAppService.Data.VOs.User
{
	public class UpdateLoggedUserVO
	{
		[Required]
		public string Password { get; set; }

		[Required]
		public Names Name { get; set; }

		public class Names
		{
			[Required]
			public string GivenName { get; set; }

			[Required]
			public string FamilyName { get; set; }
		}
	}
}