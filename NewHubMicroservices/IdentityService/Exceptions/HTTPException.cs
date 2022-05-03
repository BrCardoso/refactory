using Commons.Enums;

using System;
using System.Net;

namespace IdentityService.Exceptions
{
	public class HTTPException : Exception
	{
		public HttpStatusCode StatusCode { get; protected set; }
		public MessageCode MessageCode { get; protected set; }

		public HTTPException(HttpStatusCode statusCode, MessageCode messageCode)
		{
			StatusCode = statusCode;
			MessageCode = messageCode;
		}
	}
}