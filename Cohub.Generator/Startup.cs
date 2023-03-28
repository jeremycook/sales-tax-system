using Cohub.Data;
using Cohub.Data.Configuration;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SiteKit;
using SiteKit.AspNetCore.Views;
using SiteKit.EntityFrameworkCore;
using SiteKit.Jwt;
using SiteKit.Users;
using System.Security.Claims;

namespace Cohub.Generator
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            services.AddHttpContextAccessor();
            services.AddScoped(svc => svc.GetRequiredService<IHttpContextAccessor>().HttpContext is HttpContext httpContext ?
                httpContext.User :
                new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(JwtClaimTypes.sub, StandardUserId.System.ToString()),
                    new Claim(JwtClaimTypes.name, "System")
                })));

            services.AddSingleton(new CandidateDbContextTypes(new[] { typeof(CohubDbContext) }));
            services.AddSiteKit(Configuration);
            services.AddCohubData(Configuration);

            services.AddScoped<IRazorPartialToStringRenderer, RazorPartialToStringRenderer>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
