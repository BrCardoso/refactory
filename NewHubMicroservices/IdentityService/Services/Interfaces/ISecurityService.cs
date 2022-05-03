namespace IdentityService.Services.Interfaces
{
	public interface ISecurityService
	{
		string EncryptToSHA256(string password);
	}
}