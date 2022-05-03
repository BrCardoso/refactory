using Commons.Enums;

using IdentityService.Data.Converters.Interfaces;
using IdentityService.Data.VOs.User;
using IdentityService.Exceptions;
using IdentityService.Models;

using System.Net;

namespace IdentityService.Data.Converters.User
{
	public class UpdateUserConverter : IParser<UpdateUserVO, UserModel>
	{
		public UserModel Parse(UpdateUserVO origin)
		{
			return origin switch
			{
				null => throw new HTTPException(HttpStatusCode.BadRequest, MessageCode.API_ORIGIN_IN_CONVERTER_IS_NULL),
				_ => new UserModel
				{
					Guid = origin.Id.Value,
					UserName = origin.UserName,
					Emails = origin.Emails,
					Name = new UserModel.NameModel
					{
						FamilyName = origin.Name.FamilyName,
						GivenName = origin.Name.GivenName
					},
					Permissions = new UserModel.PermissionsModel
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
					}
				}
			};
		}
	}
}