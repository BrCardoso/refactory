namespace LoginAppService.Services.Interfaces
{
	public interface ISecurityService
	{
		string EncryptText(string text, string key);

		bool TryDecryptText(string text, string key, out string result);

		string EncodeURLSafe(string toEncode);

		string DecodeURLSafe(string encoded);
	}
}