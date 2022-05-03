using IdentityService.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace IdentityService.Filters
{
	public class HTTPResponseFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			if (context.Exception is ResponseHTTPException ex)
			{
				context.HttpContext.Response.ContentType = "application/json";
				context.Result = new ObjectResult(new
				{
					ex.StatusCode,
					ex.MessageCode,
					ex.Response
				})
				{ StatusCode = (int)ex.StatusCode };
			}
		}
	}
}