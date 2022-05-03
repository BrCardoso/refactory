using IdentityService.Exceptions;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System.Net;

namespace IdentityService.Filters
{
	public class HTTPFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			if (context.Exception is HTTPException ex)
			{
				context.HttpContext.Response.ContentType = "application/json";
				context.Result = new ObjectResult(new
				{
					ex.StatusCode,
					ex.MessageCode
				})
				{ StatusCode = (int)HttpStatusCode.BadRequest };
			}
		}
	}
}