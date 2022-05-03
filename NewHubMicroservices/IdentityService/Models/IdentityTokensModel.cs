using System;
using System.Collections.Generic;

namespace IdentityService.Models
{
	public class IdentityTokensModel
	{
		public Guid Guid { get; set; }
		public string DocType { get; set; } = "IdentityTokens";
		public List<TokenModel> Tokens { get; set; }
	}
}