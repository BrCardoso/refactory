using Commons.Enums;

using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Data.VOs.User
{
	public class CreateUserVO
	{
		[Required]
		public string UserName { get; set; }

		[Required]
		[StringLength(30, MinimumLength = 6)]
		public string Password { get; set; }

		[Required]
		public PermissionsNewVO Permissions { get; set; }

		[Required]
		public NameNewVO Name { get; set; }

		[Required]
		public List<string> Emails { get; set; }

		public class PermissionsNewVO
		{
			[Required]
			public bool RegularizePendingIssues { get; set; }

			[Required]
			public string AccessLevel { get; set; }

			[Required]
			public bool MakeInvoiceConference { get; set; }

			[Required]
			public bool ChangeOtherUsers { get; set; }

			[Required]
			public bool AccessToBI { get; set; }

			[Required]
			public string Cnpj { get; set; }

			[Required]
			public UserType UserType { get; set; }

			[Required]
			public bool MakeLoadRequest { get; set; }

			[Required]
			public bool ApproveNewLogins { get; set; }
		}

		public class NameNewVO
		{
			[Required]
			public string GivenName { get; set; }

			[Required]
			public string FamilyName { get; set; }
		}
	}
}