using Commons.Enums;

using System.Net;

namespace IdentityService.Exceptions
{
	public class ResponseHTTPException : HTTPException
	{
		public object Response { get; private set; }

		public ResponseHTTPException(HttpStatusCode statusCode, MessageCode messageCode, object response) : base(statusCode, messageCode)
		{
			Response = response;
		}
	}
}