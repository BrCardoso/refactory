using Couchbase.Extensions.DependencyInjection;

using LoginAppService.Repository;
using LoginAppService.Repository.Interfaces;
using LoginAppService.Services;
using LoginAppService.Services.Interfaces;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Authentication;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace LoginAppService
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		public void ConfigureServices(IServiceCollection services)
		{
			services.AddControllers()
				.AddJsonOptions(x =>
				{
					x.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
				});

			services.AddSwaggerGen(setup =>
			{
				setup.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Login API Docs v1",
					Description = "Breve documentaçao das endpoints da API do Login"
				});
				setup.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
				{
					Description = "Informe as credenciais para ter acesso a API. Exemplo: Bearer XXXXXXXX",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Scheme = "Bearer",
					Type = SecuritySchemeType.ApiKey
				});
				setup.AddSecurityRequirement(new OpenApiSecurityRequirement()
				{
					{
					  new OpenApiSecurityScheme
					  {
						Reference = new OpenApiReference
						  {
							Type = ReferenceType.SecurityScheme,
							Id = "Bearer"
						  },
						  Scheme = "oauth2",
						  Name = "Bearer",
						  In = ParameterLocation.Header,

						},
						new List<string>()
					  }
				});
			});

			services.AddCouchbase(Configuration.GetSection("Couchbase"));

			services.AddHttpClient<IServiceCollection>()
			.ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
			{
				AllowAutoRedirect = false,
				ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
				SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
			});

			services.Configure<Models.SMTPSettings>(Configuration.GetSection("SMTPSettings"));

			//Serivices
			services.AddScoped<ISendEmailService, SendEmailService>();
			services.AddSingleton<ISecurityService, SecurityService>();
			services.AddScoped<IConfigPasswordService, ConfigPasswordService>();
			services.AddScoped<IUserService, UserService>();

			//Repositories
			services.AddScoped<IConfigPasswordRepository, ConfigPasswordRepository>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseCorsMiddleware();

			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				app.UseHsts();
			}

			app.UseSwagger();

			app.UseSwaggerUI(c =>
			{
				c.RoutePrefix = "docs";
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Login API Docs V1");
			});

			app.UseHttpsRedirection();
			app.UseStaticFiles();

			app.UseRouting();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapGet("/", async context =>
				{
					await context.Response.WriteAsync($"Hello World! This a authentication API.");
				});
				endpoints.MapControllers();
			});
		}
	}

	public class CorsMiddleware
	{
		private readonly RequestDelegate _next;

		public CorsMiddleware(RequestDelegate next)
		{
			_next = next;
		}

		public Task Invoke(HttpContext httpContext)
		{
			httpContext.Response.Headers.Add("Access-Control-Allow-Origin", "*");
			httpContext.Response.Headers.Add("Access-Control-Allow-Methods", "POST,GET,PUT,PATCH,DELETE,OPTIONS");
			httpContext.Response.Headers.Add("Access-Control-Allow-Headers", "*");
			httpContext.Response.Headers.Add("Access-Control-Expose-Headers", "*");

			if (httpContext.Request.Method == "OPTIONS")
			{
				httpContext.Response.StatusCode = 200;
				return httpContext.Response.WriteAsync("OK");
			}
			return _next(httpContext);
		}
	}

	// Extension method used to add the middleware to the HTTP request pipeline.
	public static class CorsMiddlewareExtensions
	{
		public static IApplicationBuilder UseCorsMiddleware(this IApplicationBuilder builder)
		{
			return builder.UseMiddleware<CorsMiddleware>();
		}
	}

	public class MyMiddleware
	{
		private readonly RequestDelegate next;

		public MyMiddleware(RequestDelegate next)
		{
			this.next = next;
		}

		public async Task Invoke(HttpContext context)
		{
			this.BeginInvoke(context);
			await this.next.Invoke(context);
			this.EndInvoke(context);
		}

		private void BeginInvoke(HttpContext context)
		{
			// Do custom work before controller execution
		}

		private void EndInvoke(HttpContext context)
		{
			// Do custom work after controller execution
		}
	}

	public static class HttpRequestExtension
	{
		public static string GetHeader(this HttpRequest request, string key)
		{
			return request.Headers.FirstOrDefault(x => x.Key == key).Value.FirstOrDefault();
		}
	}
}