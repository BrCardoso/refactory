using LoginAppService.Models;
using LoginAppService.Services.Interfaces;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;

namespace LoginAppService.Services
{
	public class SendEmailService : ISendEmailService
	{
		private readonly IOptions<SMTPSettings> _emailSettings;

		public SendEmailService(IOptions<SMTPSettings> emailSettings)
		{
			_emailSettings = emailSettings;
		}

		public void SendEmailAsync(string mailTo, string subject, string body)
		{
			MailMessage mail = InitializeConfigAsync(subject, body);
			mail.IsBodyHtml = true;
			//htmlBody is a string containing the entire body text
			var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
			//This does the trick
			htmlView.ContentType.CharSet = Encoding.UTF8.WebName;
			mail.AlternateViews.Add(htmlView);
			mail.To.Add(new MailAddress(mailTo));

			FinishAndSendAsync(mail);
		}

		public void SendEmailAsync(string[] mailsTo, string subject, string body)
		{
			MailMessage mail = InitializeConfigAsync(subject, body);
			mail.IsBodyHtml = true;
			//htmlBody is a string containing the entire body text
			var htmlView = AlternateView.CreateAlternateViewFromString(body, new ContentType("text/html"));
			//This does the trick
			htmlView.ContentType.CharSet = Encoding.UTF8.WebName;
			mail.AlternateViews.Add(htmlView);

			foreach (string email in mailsTo)
				mail.To.Add(new MailAddress(email));

			FinishAndSendAsync(mail);
		}

		private MailMessage InitializeConfigAsync(string subject, string body)
		{
			return new MailMessage()
			{
				From = new MailAddress(_emailSettings.Value.Login),
				Subject = subject,
				Body = body,
				IsBodyHtml = true
			};
		}

		private void FinishAndSendAsync(MailMessage mail)
		{
			var smtp = new SmtpClient(_emailSettings.Value.Domain, _emailSettings.Value.Port)
			{
				Credentials = new NetworkCredential(_emailSettings.Value.Login, _emailSettings.Value.Password),
				EnableSsl = true
			};

			Task.Run(async () =>
			{
				await smtp.SendMailAsync(mail);

			});
		}
	}
}