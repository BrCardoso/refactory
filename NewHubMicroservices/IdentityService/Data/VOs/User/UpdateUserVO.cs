using Commons.Enums;

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace IdentityService.Data.VOs.User
{
	public class UpdateUserVO
	{
		[Required]
		public Guid? Id { get; set; }

		[Required]
		public string UserName { get; set; }

		[StringLength(30, MinimumLength = 6)]
		public string Password { get; set; }

		[Required]
		public PermissionsUpVO Permissions { get; set; }

		[Required]
		public NameUpVO Name { get; set; }

		[Required]
		public List<string> Emails { get; set; }

		public class PermissionsUpVO
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

			public UserType UserType { get; set; }

			[Required]
			public bool MakeLoadRequest { get; set; }

			[Required]
			public bool ApproveNewLogins { get; set; }

			[Required]
			public UserStatus Status { get; set; }
		}

		public class NameUpVO
		{
			public string GivenName { get; set; }
			public string FamilyName { get; set; }
		}
	}
}