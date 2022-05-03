using Commons.Enums;

using IdentityService.Data.Converters.Interfaces;
using IdentityService.Data.VOs.User;
using IdentityService.Exceptions;
using IdentityService.Models;

using System.Collections.Generic;
using System.Linq;
using System.Net;

namespace IdentityService.Data.Converters.User
{
	public class UserConverter : IParser<UserModel, UserVO>, IParserList<UserModel, UserVO>
	{
		public UserVO Parse(UserModel origin)
		{
			return origin switch
			{
				null => throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.API_ORIGIN_IN_CONVERTER_IS_NULL),
				_ => new UserVO
				{
					Id = origin.Guid,
					UserName = origin.UserName,
					Emails = origin.Emails,
					Name = new UserVO.NameVO
					{
						FamilyName = origin.Name.FamilyName,
						GivenName = origin.Name.GivenName
					},
					Permissions = new UserVO.PermissionsVO
					{
						AccessLevel = origin.Permissions.AccessLevel,
						AccessToBI = origin.Permissions.AccessToBI,
						ApproveNewLogins = origin.Permissions.ApproveNewLogins,
						ChangeOtherUsers = origin.Permissions.ChangeOtherUsers,
						Cnpj = origin.Permissions.Cnpj,
						MakeInvoiceConference = origin.Permissions.MakeInvoiceConference,
						MakeLoadRequest = origin.Permissions.MakeLoadRequest,
						RegularizePendingIssues = origin.Permissions.RegularizePendingIssues,
						Status = origin.Permissions.Status,
						UserType = origin.Permissions.UserType
					},
					Meta = new UserVO.MetaVO
					{
						Created = origin.Meta.Created,
						LastModified = origin.Meta.LastModified
					}
				}
			};
		}

		public IEnumerable<UserVO> Parse(IEnumerable<UserModel> origin)
		{
			return origin switch
			{
				null => throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.API_ORIGIN_IN_CONVERTER_IS_NULL),
				_ => origin.Select(x => Parse(x))
			};
		}
	}
}