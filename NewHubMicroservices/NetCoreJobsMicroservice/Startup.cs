using Couchbase.Extensions.DependencyInjection;

using NetCoreJobsMicroservice.Converters;
using NetCoreJobsMicroservice.Models;
using NetCoreJobsMicroservice.Repository;
using NetCoreJobsMicroservice.Repository.Interfaces;
using NetCoreJobsMicroservice.Services;
using NetCoreJobsMicroservice.Services.Interfaces;
using NetCoreJobsMicroservice.Services.Interfaces.Requests;
using NetCoreJobsMicroservice.Services.Requests;

using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Collections.Generic;
using System.Net.Http;
using System.Security.Authentication;
using System.Threading.Tasks;
using NetCoreJobsMicroservice.Repository.Interface;
using System.Linq;
using NetCoreJobsMicroservice.Business;
using NetCoreJobsMicroservice.Business.Interfaces;

namespace NetCoreJobsMicroservice
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
            services.AddControllersWithViews();

            services.AddControllers().AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.Converters.Add(new DoubleConverter());
            });

            // tag::ConfigureServices[]
            services.AddCouchbase(Configuration.GetSection("Couchbase"));
            // end::ConfigureServices[]

            services.AddHttpClient<IServiceCollection>()
                .ConfigurePrimaryHttpMessageHandler(sp => new HttpClientHandler
                {
                    AllowAutoRedirect = false,
                    ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true,
                    SslProtocols = SslProtocols.Tls | SslProtocols.Tls11 | SslProtocols.Tls12
                });

            services.Configure<List<BucketName>>(Configuration.GetSection("Couchbase:Buckets"));

            //Services
            services.AddScoped<IConferenceRequestService, ConferenceRequestService>();
            services.AddScoped<IConferenceService, ConferenceService>();
            services.AddSingleton<IConferenceFinishService, ConferenceFinishService>();
            services.AddSingleton<IConferenceCompareBenefitiaryService, ConferenceCompareBenefitiaryService>();
            services.AddSingleton<IProviderRequestService, ProviderRequestService>();

            services.AddSingleton<IBeneficiaryRequestService, BeneficiaryRequestService>();
            services.AddSingleton<IChargeRequestService, ChargeRequestService>();
            services.AddSingleton<ICompanyRequestService, CompanyRequestService>();
            services.AddSingleton<IEmployeeRequestService, EmployeeRequestService>();
            services.AddSingleton<IHubCustomerRequestService, HubCustomerRequestService>();
            services.AddSingleton<INITRequestService, NITRequestService>();
            services.AddSingleton<IProviderRequestService, ProviderRequestService>();

            //Repositories
            services.AddScoped<IConferenceRepository, ConferenceRepository>();
            services.AddScoped<IEventsRepository, EventsRepository>();
            services.AddScoped<IFamilyRepository, FamilyRepository>();
            services.AddScoped<IRulesConfigurationRepository, RulesConfigurationRepository>();
            services.AddScoped<ITaskPanelRepository, TaskPanelRepository>();
            services.AddScoped<IMandatoryRulesRepository, MandatoryRulesRepository>();
            services.AddScoped<IChargeBusiness, ChargeBusiness>();
            services.AddScoped<IMovimentQueueBusiness, MovimentQueueBusiness>();

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
                    await context.Response.WriteAsync($"Hello World! This a jobs API.");
                });

                endpoints.MapGet("/entity/", async context =>
                {
                    await context.Response.WriteAsync("Ops");
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