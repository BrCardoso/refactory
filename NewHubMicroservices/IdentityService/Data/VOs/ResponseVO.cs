using Commons.Enums;

using System.Net;

namespace IdentityService.Data.VOs
{
	public class ResponseVO
	{
		public HttpStatusCode StatusCode { get; set; }
		public MessageCode MessageCode { get; set; }
		public object Response { get; set; }
	}
}