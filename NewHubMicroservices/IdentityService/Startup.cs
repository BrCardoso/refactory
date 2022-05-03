using Couchbase.Extensions.DependencyInjection;

using IdentityService.Business;
using IdentityService.Business.Interfaces;
using IdentityService.Data.Converters.Auth;
using IdentityService.Data.Converters.User;
using IdentityService.Exceptions;
using IdentityService.Filters;
using IdentityService.MiddleWares;
using IdentityService.Repository;
using IdentityService.Repository.Interfaces;
using IdentityService.Services;
using IdentityService.Services.Interfaces;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;

namespace IdentityService
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
					x.JsonSerializerOptions.IgnoreNullValues = true;
				})
				.AddMvcOptions(x =>
				{
					x.Filters.Add<HTTPResponseFilter>(1);
					x.Filters.Add<HTTPFilter>(2);
					x.Filters.Add<GlobalFilter>(3);
				});

			// JWT Authentication
			services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
				.AddJwtBearer(options => options.TokenValidationParameters = new TokenValidationParameters
				{
					ValidateIssuer = false,
					ValidateAudience = false,

					ValidateLifetime = true,

					ValidateIssuerSigningKey = true,
					IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(Configuration["JWT:Key"]))
				});

			services.AddSwaggerGen(setup =>
			{
				setup.SwaggerDoc("v1", new OpenApiInfo
				{
					Version = "v1",
					Title = "Identity API Docs v1",
					Description = "Breve documentaçao das endpoints da API do Identity"
				});
				setup.AddSecurityDefinition("Basic", new OpenApiSecurityScheme
				{
					Description = "Informe as credenciais para ter acesso a API",
					Name = "Authorization",
					In = ParameterLocation.Header,
					Scheme = "basic",
					Type = SecuritySchemeType.Http
				});
				setup.AddSecurityRequirement(new OpenApiSecurityRequirement
				{
					{
						new OpenApiSecurityScheme
						{
							Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Basic" }
						},
						new List<string>()
					}
				});

				//setup.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, $"{Assembly.GetExecutingAssembly().GetName().Name}.xml"), true);
			});

			services.AddCouchbase(Configuration.GetSection("Couchbase"));

			//Businesses
			services.AddScoped<IUserBusiness, UserBusiness>();
			services.AddScoped<IAuthBusiness, AuthBusiness>();

			//Repositories
			services.AddScoped<IUserRepository, UserRepository>();
			services.AddScoped<IIdentitySettingsRepository, IdentitySettingsRepository>();
			services.AddScoped<IIdentityTokensRepository, IdentityTokensRepository>();

			//Services
			services.AddSingleton<ISecurityService, SecurityService>();
			services.AddSingleton<ITokenService, TokenService>();

			//Converters
			services.AddSingleton<UserConverter>();
			services.AddSingleton<CreateUserConverter>();
			services.AddSingleton<UpdateUserConverter>();
			services.AddSingleton<TokenConverter>();
		}

		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			app.UseMiddleware<RequestAuthorizationMiddleware>();

			if (env.IsDevelopment())
				app.UseDeveloperExceptionPage();

			app.UseSwagger();

			app.UseSwaggerUI(c =>
			{
				c.RoutePrefix = "docs";
				c.SwaggerEndpoint("/swagger/v1/swagger.json", "Identity API Docs V1");
			});

			//app.UseHttpsRedirection();

			app.UseRouting();

			app.UseAuthentication();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllers();
			});
		}
	}
}