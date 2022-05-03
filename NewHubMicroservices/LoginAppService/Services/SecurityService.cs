using LoginAppService.Services.Interfaces;

using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace LoginAppService.Services
{
	public class SecurityService : ISecurityService
	{
		private readonly byte[] _initVector =
		{
			0xeb, 0x99, 0x94, 0xb0, 0xf2, 0x99, 0x3e, 0xbe, 0x6d, 0xba, 0x49, 0x50, 0xed, 0x25, 0xec, 0x44
		};

		public string EncryptText(string text, string key)
		{
			using var aes = Aes.Create();
			aes.Mode = CipherMode.CBC;

			byte[] aesKey = new byte[32];
			Array.Copy(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);
			aes.Key = aesKey;
			aes.IV = _initVector;

			using var memoryStream = new MemoryStream();
			ICryptoTransform crypto = aes.CreateEncryptor();

			using var cryptoStream = new CryptoStream(memoryStream, crypto, CryptoStreamMode.Write);
			byte[] plainBytes = Encoding.UTF8.GetBytes(text);

			cryptoStream.Write(plainBytes, 0, plainBytes.Length);

			cryptoStream.FlushFinalBlock();

			byte[] cipherBytes = memoryStream.ToArray();

			return Convert.ToBase64String(cipherBytes, 0, cipherBytes.Length);
		}

		public bool TryDecryptText(string text, string key, out string result)
		{
			try
			{
				using var aes = Aes.Create();
				aes.Mode = CipherMode.CBC;

				byte[] aesKey = new byte[32];
				Array.Copy(new SHA256Managed().ComputeHash(Encoding.UTF8.GetBytes(key)), 0, aesKey, 0, 32);
				aes.Key = aesKey;
				aes.IV = _initVector;

				using var memoryStream = new MemoryStream();
				ICryptoTransform decrypto = aes.CreateDecryptor();

				var cryptoStream = new CryptoStream(memoryStream, decrypto, CryptoStreamMode.Write);
				byte[] cipherBytes = Convert.FromBase64String(text);

				cryptoStream.Write(cipherBytes, 0, cipherBytes.Length);

				cryptoStream.FlushFinalBlock();

				byte[] plainBytes = memoryStream.ToArray();

				result = Encoding.UTF8.GetString(plainBytes, 0, plainBytes.Length);

				return true;
			}
			catch
			{
				result = null;
				return false;
			}
		}

		public string EncodeURLSafe(string toEncode)
		{
			string encoded = Convert.ToBase64String(Encoding.UTF8.GetBytes(toEncode)).Replace('+', '-').Replace('/', '_');
			encoded = encoded.TrimEnd('=');

			return encoded;
		}

		public string DecodeURLSafe(string encoded)
		{
			List<char> chars = new List<char>(encoded.ToCharArray());

			for (int i = 0; i < chars.Count; ++i)
			{
				if (chars[i] == '_')
					chars[i] = '/';
				else if (chars[i] == '-')
					chars[i] = '+';
			}

			switch (encoded.Length % 4)
			{
				case 2:
					chars.AddRange(new[] { '=', '=' });
					break;

				case 3:
					chars.Add('=');
					break;
			}

			char[] array = chars.ToArray();

			try
			{
				return Encoding.UTF8.GetString(Convert.FromBase64CharArray(array, 0, array.Length));
			}
			catch
			{
				return null;
			}
		}
	}
}