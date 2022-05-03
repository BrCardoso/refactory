using System.Collections.Generic;

namespace IdentityService.Models
{
	public class IdentitySettingsModel
	{
		public string Guid { get; set; }

		public string DocType { get; set; }

		public SettingsModel Settings { get; set; }

		public class SettingsModel
		{
			public List<UserModel> Users { get; set; }

			public class UserModel
			{
				public string Email { get; set; }

				public string Password { get; set; }

				public string Type { get; set; }
			}
		}
	}
}