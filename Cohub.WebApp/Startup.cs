using Cohub.Data;
using Cohub.Data.Configuration;
using Cohub.Data.Sto.ValidationEndpoint;
using Cohub.Data.Usr;
using Cohub.WebApp.Configuration;
using Hangfire;
using Hangfire.Dashboard;
using Hangfire.PostgreSql;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.CookiePolicy;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using SiteKit;
using SiteKit.AspNetCore;
using SiteKit.AspNetCore.Routing;
using SiteKit.EntityFrameworkCore;
using SiteKit.Files;
using SiteKit.Jwt;
using SiteKit.Users;
using SoapCore;
using StackExchange.Profiling;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Security.Claims;
using System.ServiceModel;

namespace Cohub.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;

            Configuration["Buster"] = Program.Seed.ToString();
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            if (!Environment.IsDevelopment())
            {
                services.AddHttpsRedirection(options => options.HttpsPort = 443);
            }

            // Add Hangfire services and IHostedService.
            services.AddHangfire(configuration => configuration
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_170)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSerilogLogProvider()
                .UsePostgreSqlStorage(Configuration.GetConnectionString("Hangfire"), new PostgreSqlStorageOptions()));
            services.AddHangfireServer();
            services.AddScoped<RecurringJobs>();

            if (Program.Debug)
            {
                services.AddMiniProfiler(options => options.IgnorePath("/hangfire")).AddEntityFramework();
            }

            services.AddHttpContextAccessor();
            services.AddScoped(svc => svc.GetRequiredService<IHttpContextAccessor>().HttpContext is HttpContext httpContext ?
                httpContext.User :
                new ClaimsPrincipal(new ClaimsIdentity(new[]
                {
                    new Claim(JwtClaimTypes.sub, StandardUserId.System.ToString()),
                    new Claim(JwtClaimTypes.name, "System")
                })));

            services.AddPortableObjectLocalization(options => options.ResourcesPath = "Localization");
            services.Configure<RequestLocalizationOptions>(options =>
            {
                var supportedCultures = new List<CultureInfo>
                {
                    new CultureInfo("en"),
                    new CultureInfo("es"),
                };

                options.DefaultRequestCulture = new RequestCulture("en-US");
                // Commented out because we are only translating text: options.SupportedCultures = supportedCultures;
                options.SupportedUICultures = supportedCultures;
            });

            services.AddSingleton<IContentTypeProvider>(new FileExtensionContentTypeProvider()
            {
                Mappings =
                {
                    [".riot"] = "text/plain",
                }
            });

            services.Configure<FileManagerOptions>(Configuration.GetSection("FileManager"));
            services.AddSingleton<FileManager>();

            services.AddSingleton(new CandidateDbContextTypes(new[] { typeof(CohubDbContext) }));
            services.AddSiteKit(Configuration);
            services.AddCohubData(Configuration, Environment);
            services.AddCohubWebApp(Configuration);

            services.AddSoapCore();
            services.AddScoped<RemoteAcctValidationSvc>();

            services.Configure<CookiePolicyOptions>(options =>
            {
                options.MinimumSameSitePolicy = SameSiteMode.Lax;
                options.Secure = CookieSecurePolicy.Always;
                options.HttpOnly = HttpOnlyPolicy.Always;
            });

            services.AddControllersWithViews(o =>
            {
                o.Filters.Add(new AuthorizeFilter(Policy.Internal));
                o.ModelMetadataDetailsProviders.Add(new SiteKitDisplayMetadataProvider());
                o.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
            })
                .AddViewLocalization(LanguageViewLocationExpanderFormat.Suffix)
                .AddDataAnnotationsLocalization();

            services.AddRazorPages(options =>
            {
                options.Conventions.Add(new PageRouteTransformerConvention(new SlugifyParameterTransformer()));
            });

            services.AddAuthorization(options =>
            {
                foreach (var item in Policy.Roles)
                {
                    options.AddPolicy(item.Key, new AuthorizationPolicyBuilder()
                        .RequireRole(item.Value)
                        .Build());
                }

                var internalPolicy = options.GetPolicy(Policy.Internal)!;
                options.DefaultPolicy = internalPolicy;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILogger<Startup> logger, IRecurringJobManager recurringJobManager)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/error");
            }

            app.UseHangfireServer();
            RecurringJobs.Schedule(recurringJobManager);

            try
            {
                using var scope = app.ApplicationServices.CreateScope();
                using var db = scope.ServiceProvider.GetRequiredService<CohubDbContext>();
                if (db.Database.GetPendingMigrations() is var val && val.Any())
                {
                    var pendingMigrations = val.ToArray();
                    db.Database.Migrate();
                    logger.LogWarning($"Applied pending database migrations during startup: {string.Join(", ", pendingMigrations)}.");
                }

                db.AddMissingRange(new[]
                {
                    new Role { Id = RoleId.Disabled, IsActive = true },

                    new Role { Id = RoleId.Super, IsActive = false, Description = "Unrestricted access to entire system." },

                    new Role { Id = RoleId.Manager, IsActive = true, Description = "Unrestricted access to sales tax system." },
                    new Role { Id = RoleId.Processor, IsActive = true, Description = "Read only and restricted edit access to sales tax system." },
                    new Role { Id = RoleId.Auditor, IsActive = true, Description = "Read only and restricted edit access to sales tax system." },
                    new Role { Id = RoleId.Reviewer, IsActive = true, Description = "Read only access to sales tax system." },
                    new Role { Id = RoleId.LicenseReviewer, IsActive = true, Description = "Read only access to business and license information." },
                });
                db.SaveChanges();
            }
            catch (Exception ex)
            {
                logger.LogCritical(ex, $"Suppressed error applying database migrations during startup {ex.GetType()}: {ex.Message}");
            }

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedHost | ForwardedHeaders.XForwardedProto
            });

            app.UseHttpsRedirection();

            //app.Use((ctx, next) =>
            //{
            //    ctx.Response.Headers.Add("Content-Security-Policy", "default-src 'self';");
            //    return next();
            //});

            app.UseStaticFiles(new StaticFileOptions
            {
                ContentTypeProvider = app.ApplicationServices.GetRequiredService<IContentTypeProvider>()
            });

            app.UseSerilogRequestLogging(o => o.EnrichDiagnosticContext = LogHelper.EnrichFromRequest);

            app.UseRouting();
            app.UseRequestLocalization();

            app.UseAuthentication();
            app.UseAuthorization();

            if (Program.Debug)
            {
                app.UseMiniProfiler();
            }

            app.UseEndpoints(endpoints =>
            {
                endpoints.UseSoapEndpoint<RemoteAcctValidationSvc>("/sto/soap11/validationSvc.asmx", new BasicHttpBinding());

                endpoints.MapHangfireDashboard(new DashboardOptions
                {
                    Authorization = new[] { new HangfireAuthorizationFilter() }
                });
                endpoints.MapControllerRoute(
                    name: "areas",
                    pattern: "{area:exists}/{controller=Home}/{action=Index}/{id?}");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });
        }

        private class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
        {
            private const string localhost = "127.0.0.1";

            public bool Authorize(DashboardContext context)
            {
                HttpContext httpContext = context.GetHttpContext();
                if (httpContext.User.IsInRole(RoleId.Manager) ||
                    httpContext.User.IsInRole(RoleId.Super) ||
                    httpContext.Request.Host.Host == localhost)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
        }

        public static class LogHelper
        {
            public static void EnrichFromRequest(IDiagnosticContext diagnosticContext, HttpContext httpContext)
            {
                var request = httpContext.Request;

                // Set all the common properties available for every request
                diagnosticContext.Set("Host", request.Host);
                diagnosticContext.Set("Protocol", request.Protocol);
                diagnosticContext.Set("Scheme", request.Scheme);
                diagnosticContext.Set("RemoteIpAddress", httpContext.Connection.RemoteIpAddress);

                // Only set it if available. You're not sending sensitive data in a querystring right?!
                if (request.QueryString.HasValue)
                {
                    diagnosticContext.Set("QueryString", request.QueryString.Value);
                }

                // Set the content-type of the Response at this point
                diagnosticContext.Set("ContentType", httpContext.Response.ContentType);

                if (httpContext.User.Sub() is string sub)
                {
                    diagnosticContext.Set("Sub", sub);
                }

                // Retrieve the IEndpointFeature selected for the request
                var endpoint = httpContext.GetEndpoint();
                if (endpoint is object) // endpoint != null
                {
                    diagnosticContext.Set("EndpointName", endpoint.DisplayName);
                }
            }
        }
    }
}
