namespace LoginAppService.Models
{
	public class SMTPSettings
	{
		public string Domain { get; set; }
		public int Port { get; set; }
		public string Login { get; set; }
		public string Password { get; set; }
	}
}