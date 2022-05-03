using Commons.Base;
using System;
using System.Collections.Generic;
using static Commons.Helpers;

namespace Commons.Models.Operations.WSO2
{
    public class IdentityWSO2
    {
        public string access_token { get; set; }
        public string refresh_token { get; set; }
        public string scope { get; set; }
        public string id_token { get; set; }
        public string token_type { get; set; }
        public int expires_in { get; set; }
    }

    public class ValidaToken
    {
        public bool active { get; set; }
        public string nbf { get; set; }
        public string scope { get; set; }
        public string token_type { get; set; }
        public string exp { get; set; }
        public string iat { get; set; }
        public string client_id { get; set; }
        public string username { get; set; }
    }

    public class RefreshTokenError
    {
        public string error { get; set; }
        public string error_description { get; set; }
    }

    public class ApplicationResponseModel : MethodFeedback
    {
        public IdentityWSO2 credentials { get; set; }
        public Guid customer { get; set; }
        public string customerName { get; set; }
        public string accesslevel { get; set; }
        public string group { get; set; }
        public string username { get; set; }
        public string email { get; set; }
        public string accessToBI { get; set; }
        public string cnpj { get; set; }
        public string makeLoadRequest { get; set; }
        public string approveNewLogins { get; set; }
        public string family_name { get; set; }
        public string given_name { get; set; }
    }
    
    public class Auth
    {
        public class RequestModel
        {
            public string login { get; set; }

            public string password { get; set; }
        }

        public class RefreshTokenModel
        {
            [NotEmpty]
            public string refreshToken { get; set; }
        }

        public class ApplicationResponseModel : MethodFeedback
        {
            public string customer { get; set; }

            public string accesslevel { get; set; }

            public List<HierarchyGroup> groups { get; set; }

            public string username { get; set; }

            public string email { get; set; }
            public string accessToBI { get; set; }
            public string cnpj { get; set; }
            public string makeLoadRequest { get; set; }
            public string approveNewLogins { get; set; }
        }
    }


    public class Login
    {
        public class RequestModel
        {
            [NotEmpty]
            public string login { get; set; }

            [NotEmpty]
            public string password { get; set; }
        }

        public class WSO2ResponseModel
        {
            public string access_token { get; set; }

            public string refresh_token { get; set; }

            public string scope { get; set; }

            public string id_token { get; set; }

            public string token_type { get; set; }

            public int expires_in { get; set; }
        }

        public class ApplicationResponseModel : MethodFeedback
        {
            public WSO2ResponseModel credentials { get; set; }

            public Guid customer { get; set; }

            public string customerName { get; set; }

            public string accesslevel { get; set; }

            public string group { get; set; }

            public string username { get; set; }

            public string email { get; set; }
            public string accessToBI { get; set; }
            public string cnpj { get; set; }
            public string makeLoadRequest { get; set; }
            public string approveNewLogins { get; set; }

            public string family_name { get; set; }

            public string given_name { get; set; }
        }
    }
}
