using Commons;

using System;

using static Commons.Helpers;

namespace LoginAppService.Model
{
    public class Login
	{
		public class RequestModel
		{
			[NotEmpty]
			public string login { get; set; }

			[NotEmpty]
			public string password { get; set; }
		}

		public class  IdentityRequestModel
		{
			public string accessToken { get; set; }

			public string refreshToken { get; set; }

			public string token { get; set; }

			public int expiresIn { get; set; }
		}

		public class IdentityResponseModel
		{
			public string access_token { get; set; }
			public string refresh_token { get; set; }
			public string id_token { get; set; }
			public string token_type { get; set; } = "Bearer";
			public int expires_in { get; set; }
		}

		public class ApplicationResponseModel : MethodFeedback
		{
			public bool changeOtherUsers { get; set; }
			public bool makeInvoiceConference { get; set; }
			public IdentityResponseModel credentials { get; set; }
			public Guid customer { get; set; }
			public string customerName { get; set; }
			public string accesslevel { get; set; }
			public string group { get; set; }
			public string username { get; set; }
			public string email { get; set; }
			public bool accessToBI { get; set; }
			public string cnpj { get; set; }
			public bool makeLoadRequest { get; set; }
			public bool approveNewLogins { get; set; }
			public string family_name { get; set; }
			public string given_name { get; set; }
            public string typeUser { get; set; }
        }

		internal class WSO2ErrorResponseModel
		{
			public string error_description { get; set; }
			public string invalid_grant { get; set; }
		}
	}
}