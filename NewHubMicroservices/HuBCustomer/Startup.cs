using System.Linq;
using System.Threading.Tasks;
using Couchbase.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace HuBCustomerAppService
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // tag::ConfigureServices[]
            services
                    .AddCouchbase(Configuration.GetSection("Couchbase"));
            // end::ConfigureServices[]
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync($"Hello World! This a hubCustomer API. Enviroment:{Configuration.GetValue<string>("Couchbase:Servers:0")}");
                });
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
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
