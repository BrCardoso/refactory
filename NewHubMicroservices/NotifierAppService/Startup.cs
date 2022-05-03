using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Net.Http;
using System.Security.Authentication;
using Couchbase.Extensions.DependencyInjection;
using NotifierAppService.Models;
using NotifierAppService.Services.Interfaces;
using NotifierAppService.Services;
using NotifierAppService.Repository.Interfaces;
using NotifierAppService.Repository;
using NotifierAppService.Hubs;

namespace NotifierAppService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSignalR();
            services.AddControllers();

            services.AddCouchbase(Configuration.GetSection("Couchbase"));

            services.AddHttpClient<IServiceCollection>()
                .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
                });

            services.Configure<List<BucketName>>(Configuration.GetSection("Couchbase:Buckets"));

            //Serivices
            services.AddScoped<INotificationService, NotificationService>();

            //Repositories
            services.AddScoped<INotificationRepository, NotificationRepository>();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseCorsMiddleware();

            app.UseCors(builder =>
            {
                builder
                  .AllowAnyHeader()
                  .AllowCredentials()
                  .SetIsOriginAllowed(hostName => true);
            });

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
                app.UseHsts();

            //app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync($"Hello World! This a notifications API. Enviroment:{Configuration.GetValue<string>("Couchbase:Servers:0")}");
                });

                endpoints.MapGet("/entity/", async context =>
                {
                    await context.Response.WriteAsync("Ops");
                });

                endpoints.MapControllers();
                endpoints.MapHub<NotificationHub>("/api/v1/notification/hub");
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
            //httpContext.Response.Headers.Add("Access-Control-Expose-Headers", "*");

            //if (httpContext.Request.Method == "OPTIONS")
            //{
            //    httpContext.Response.StatusCode = 200;
            //    return httpContext.Response.WriteAsync("OK");
            //}
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
            BeginInvoke(context);
            await next.Invoke(context);
            EndInvoke(context);
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
}