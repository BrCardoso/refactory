using LoginAppService.Data.VOs.User;

namespace LoginAppService.Converters
{
	public class UpdateUserConverter
	{
		public static UpdateUserVO Parse(UserVO user, UpdateLoggedUserVO updateLogged)
		{
			return new UpdateUserVO
			{
				Emails = user.Emails,
				Id = user.Id,
				Name = new UpdateUserVO.NameUpVO { FamilyName = updateLogged.Name.FamilyName, GivenName = updateLogged.Name.GivenName },
				Password = updateLogged.Password,
				Permissions = new UpdateUserVO.PermissionsUpVO
				{
					AccessLevel = user.Permissions.AccessLevel,
					AccessToBI = user.Permissions.AccessToBI,
					ApproveNewLogins = user.Permissions.ApproveNewLogins,
					ChangeOtherUsers = user.Permissions.ChangeOtherUsers,
					Cnpj = user.Permissions.Cnpj,
					MakeInvoiceConference = user.Permissions.MakeInvoiceConference,
					MakeLoadRequest = user.Permissions.MakeLoadRequest,
					RegularizePendingIssues = user.Permissions.RegularizePendingIssues,
					Status = user.Permissions.Status,
					UserType = user.Permissions.UserType
				},
				UserName = user.UserName
			};
		}
	}
}