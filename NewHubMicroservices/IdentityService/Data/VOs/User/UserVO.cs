using Commons.Enums;

using System;
using System.Collections.Generic;

namespace IdentityService.Data.VOs.User
{
	public class UserVO
	{
		public Guid Id { get; set; }
		public string UserName { get; set; }
		public PermissionsVO Permissions { get; set; }
		public NameVO Name { get; set; }
		public List<string> Emails { get; set; }
		public MetaVO Meta { get; set; }

		public class PermissionsVO
		{
			public bool RegularizePendingIssues { get; set; }
			public string AccessLevel { get; set; }
			public bool MakeInvoiceConference { get; set; }
			public bool ChangeOtherUsers { get; set; }
			public bool AccessToBI { get; set; }
			public string Cnpj { get; set; }
			public UserType UserType { get; set; }
			public bool MakeLoadRequest { get; set; }
			public bool ApproveNewLogins { get; set; }
			public UserStatus Status { get; set; }
		}

		public class NameVO
		{
			public string GivenName { get; set; }
			public string FamilyName { get; set; }
		}

		public class MetaVO
		{
			public DateTime Created { get; set; }
			public DateTime LastModified { get; set; }
		}
	}
}