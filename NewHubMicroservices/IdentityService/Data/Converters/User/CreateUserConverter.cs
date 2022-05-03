using Commons.Enums;

using IdentityService.Data.Converters.Interfaces;
using IdentityService.Data.VOs.User;
using IdentityService.Exceptions;
using IdentityService.Models;

using System.Net;

namespace IdentityService.Data.Converters.User
{
	public class CreateUserConverter : IParser<CreateUserVO, UserModel>
	{
		public UserModel Parse(CreateUserVO origin)
		{
			return origin switch
			{
				null => throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.API_ORIGIN_IN_CONVERTER_IS_NULL),
				_ => new UserModel
				{
					Emails = origin.Emails,
					Name = new UserModel.NameModel
					{
						FamilyName = origin.Name.FamilyName,
						GivenName = origin.Name.GivenName
					},
					Password = origin.Password,
					UserName = origin.UserName,
					Permissions = new UserModel.PermissionsModel
					{
						AccessLevel = origin.Permissions.AccessLevel,
						AccessToBI = origin.Permissions.AccessToBI,
						ApproveNewLogins = origin.Permissions.ApproveNewLogins,
						ChangeOtherUsers = origin.Permissions.ChangeOtherUsers,
						Cnpj = origin.Permissions.Cnpj,
						UserType = origin.Permissions.UserType,
						MakeInvoiceConference = origin.Permissions.MakeInvoiceConference,
						MakeLoadRequest = origin.Permissions.MakeLoadRequest,
						RegularizePendingIssues = origin.Permissions.RegularizePendingIssues
					}
				}
			};
		}
	}
}