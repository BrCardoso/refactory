using System.Threading.Tasks;

namespace LoginAppService.Services.Interfaces
{
	public interface ISendEmailService
	{
		void SendEmailAsync(string mailTo, string subject, string body);
		void SendEmailAsync(string[] mailsTo, string subject, string body);
	}
}