using LoginAppService.Converters;
using LoginAppService.Data.VOs.User;
using LoginAppService.Model;
using LoginAppService.Repository.Interfaces;
using LoginAppService.Services.Interfaces;

using Microsoft.Extensions.Configuration;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace LoginAppService.Services
{
	public class ConfigPasswordService : IConfigPasswordService
	{
		private readonly IConfiguration _configuration;
		private readonly ISendEmailService _sendEmailService;
		private readonly ISecurityService _securityService;
		private readonly IConfigPasswordRepository _configPasswordRepository;
		private readonly IUserService _userService;

		public ConfigPasswordService(IConfiguration configuration, ISendEmailService sendEmailService, ISecurityService securityService, IConfigPasswordRepository configPasswordRepository, IUserService userService)
		{
			_configuration = configuration;
			_sendEmailService = sendEmailService;
			_securityService = securityService;
			_configPasswordRepository = configPasswordRepository;
			_userService = userService;
		}

		public async Task RequestAsync(ConfigPassword.Data configData, string email)
		{
			ConfigPassword configPasswords = await _configPasswordRepository.FindAsync();

			if (configPasswords is null)
				configPasswords = new ConfigPassword { guid = Guid.NewGuid(), requests = new List<ConfigPassword.Data> { } };

			if (configPasswords.requests.SingleOrDefault(x => x.userguid == configData.userguid && x.usedDateTime == null) is ConfigPassword.Data currentRequest)
				configPasswords.requests.Remove(currentRequest);

			configPasswords.requests.Add(configData);

			await _configPasswordRepository.UpSertAsync(configPasswords);

			string tokenEncoded = _securityService.EncodeURLSafe(_securityService.EncryptText(JsonSerializer.Serialize(configData), _configuration.GetValue<string>("ConfigPasswordKey")));

			string recoverLinkPage = $"{_configuration.GetValue<string>("UrlClientEmail")}/configuracao-de-senha?token={tokenEncoded}";

			string htmlTemplate = await File.ReadAllTextAsync("./Resources/RecoverPasswordEmailTemplate.html");
			string body = htmlTemplate.Replace("#URI#", recoverLinkPage);

			_sendEmailService.SendEmailAsync(email, "Hub do RH - Configuração de senha", body);
		}

		public async Task<bool> ChangePasswordAsync(string token, string newPassword)
		{
			if (!(_securityService.DecodeURLSafe(token) is string jsonDecoded))
				return false;

			if (!_securityService.TryDecryptText(jsonDecoded, _configuration.GetValue<string>("ConfigPasswordKey"), out string json))
				return false;

			ConfigPassword.Data configData = JsonSerializer.Deserialize<ConfigPassword.Data>(json);

			ConfigPassword configPasswords = await _configPasswordRepository.FindAsync();
			if (configPasswords is null)
				return false;

			var currentRequest = configPasswords.requests.SingleOrDefault(x => x.requestguid == configData.requestguid);
			if (currentRequest is null)
				return false;

			if (currentRequest.usedDateTime != null)
				return false;

			currentRequest.usedDateTime = DateTime.Now;

			if (!await _configPasswordRepository.UpSertAsync(configPasswords))
				return false;

			if (!(await _userService.GetUsersInfoAsync() is List<UserVO> users))
				return false;

			var user = users.SingleOrDefault(x => x.Id == configData.userguid);

			return await _userService.UpdateUserDataAsync(UpdateUserConverter.Parse(user, new UpdateLoggedUserVO
			{
				Name = new UpdateLoggedUserVO.Names { FamilyName = user.Name.FamilyName, GivenName = user.Name.GivenName },
				Password = newPassword
			})) is UserVO;
		}
	}
}