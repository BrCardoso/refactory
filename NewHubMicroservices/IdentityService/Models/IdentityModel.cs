using System;
using System.Collections.Generic;

namespace IdentityService.Models
{
	public class IdentityModel
	{
		public Guid Guid { get; set; }
		public string DocType { get; set; } = "Identity";
		public List<UserModel> Users { get; set; }
	}
}