using IdentityService.Services.Interfaces;

using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IdentityService.Services
{
	public class SecurityService : ISecurityService
	{
		public string EncryptToSHA256(string password)
		{
			using var sha = new SHA256Managed();
			var builder = new StringBuilder();

			sha.ComputeHash(new UTF8Encoding()
				.GetBytes(password)).ToList()
				.ForEach(a => builder.Append(a.ToString("x2")));

			return builder.ToString();
		}
	}
}