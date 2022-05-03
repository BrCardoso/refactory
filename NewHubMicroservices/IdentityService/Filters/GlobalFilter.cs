using Commons.Enums;

using IdentityService.Data.VOs;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

using System;
using System.Net;

namespace IdentityService.Exceptions
{
	public class GlobalFilter : IExceptionFilter
	{
		public void OnException(ExceptionContext context)
		{
			Console.WriteLine(context.Exception.ToString());
			context.HttpContext.Response.ContentType = "application/json";
			context.Result = new ObjectResult(new ResponseVO
			{
				StatusCode = HttpStatusCode.InternalServerError,
				MessageCode = MessageCode.API_INTERNAL_ERROR
			})
			{ StatusCode = (int)HttpStatusCode.InternalServerError };
		}
	}
}