using Cohub.Data;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.FileExplorer.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using SiteKit.Info;
using SiteKit.Jwt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cohub.WebApp.Configuration
{
    public static class CohubWebAppStartupExtensions
    {
        public class Authentication
        {
            [Required]
            public string? Authority { get; set; }
            [Required]
            public string? ClientId { get; set; }
            [Required]
            public string? ClientSecret { get; set; }
        }

        public static void AddCohubWebApp(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddCookiePolicy(options => options.MinimumSameSitePolicy = SameSiteMode.Strict);

            services
                .AddAuthentication(options =>
                {
                    options.DefaultScheme = CookieAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = OpenIdConnectDefaults.AuthenticationScheme;
                })
                .AddCookie(options =>
                {
                    options.AccessDeniedPath = "/session/access-denied";
                    options.LoginPath = "/session/login";
                    options.LogoutPath = "/session/logout";
                })
                .AddOpenIdConnect(options =>
                {
                    // Test OnRemoteFailure by setting this to 1 tick.
                    //options.RemoteAuthenticationTimeout = TimeSpan.FromTicks(1);

                    // Load identity provider configuration
                    var authentication = configuration.GetSection(nameof(Authentication)).Get<Authentication>() ??
                        throw new OptionsValidationException("Authentication", typeof(Authentication), new[] { "The Authentication section is required." });
                    Validator.ValidateObject(authentication, new ValidationContext(authentication), validateAllProperties: true);

                    // Configure the identity provider
                    options.Authority = authentication.Authority;
                    options.ClientId = authentication.ClientId;
                    options.ClientSecret = authentication.ClientSecret;
                    options.RequireHttpsMetadata = true;

                    // Use the authorization code flow.
                    options.ResponseType = OpenIdConnectResponseType.Code;
                    options.UsePkce = true;
                    options.AuthenticationMethod = OpenIdConnectRedirectBehavior.RedirectGet;

                    options.GetClaimsFromUserInfoEndpoint = true;
                    options.SaveTokens = true;
                    options.SignedOutRedirectUri = "/session/logged-out";

                    options.Scope.AddRange(new[] { "openid", "profile", "email" });

                    options.SecurityTokenValidator = new JwtSecurityTokenHandler
                    {
                        // Disable the built-in JWT claims mapping feature.
                        InboundClaimTypeMap = new Dictionary<string, string>()
                    };
                    // It is safe to map all because OnTicketReceived creates a new ClaimsPrincipal with specific claims
                    options.ClaimActions.MapAll();
                    // If claims were to be accepted as is this may make sense:
                    // options.ClaimActions.MapAllExcept("iss", "nbf", "exp", "aud", "nonce", "iat", "c_hash", "role");

                    options.Events.OnTicketReceived = OnTicketReceived;
                    options.Events.OnRemoteFailure = OnRemoteFailure;
                });

            services.AddScoped<FileExplorerService>();
        }

        private static async Task OnTicketReceived(TicketReceivedContext context)
        {
            // Services
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CohubWebAppStartupExtensions));
            var environment = context.HttpContext.RequestServices.GetRequiredService<IHostEnvironment>();
            var aboutOptions = context.HttpContext.RequestServices.GetRequiredService<IOptions<AboutOptions>>().Value;
            var actor = context.HttpContext.RequestServices.GetRequiredService<Actor>();
            var db = context.HttpContext.RequestServices.GetRequiredService<CohubDbContext>();
            var userRepository = context.HttpContext.RequestServices.GetRequiredService<UserRepository>();

            var user = await userRepository.FindByLoginOrBindByEmailAsync(context.Principal ?? throw new NullReferenceException("The context.Principal is null."));

            if (user is null)
            {
                context.Fail("No matching user.");
                return;
            }

            // The first user to log into the development environment gets to be a Super user
            if (environment.IsDevelopment())
            {
                if (!await db.Set<User>().AnyAsync(o => o.RoleId == RoleId.Super))
                {
                    user.RoleId = RoleId.Super;
                    db.Comment($"Assigned {user} user to the {RoleId.Super} role.", new UserMention(user));
                }
            }

            await db.SaveChangesAsync();

            var claims = CreateClaims(user);
            var identity = new ClaimsIdentity(claims, context.Principal.Identity!.AuthenticationType, "name", "role");
            context.Principal = new ClaimsPrincipal(identity);

            logger.LogInformation("Assigned principal with ID {UserId} and username {Username}.", user.Id, user.Username);
        }

        public static List<Claim> CreateClaims(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtClaimTypes.sub, user.Id.ToString()),
                new Claim(JwtClaimTypes.username, user.Username),
                new Claim(JwtClaimTypes.name, user.Name),
                new Claim(JwtClaimTypes.role, user.RoleId),
            };
            if (user.Email is not null) claims.Add(new Claim(JwtClaimTypes.email, user.Email));
            if (user.Initials is not null) claims.Add(new Claim(JwtClaimTypes.initials, user.Initials));
            if (user.LocaleId is not null) claims.Add(new Claim(JwtClaimTypes.locale, user.LocaleId));
            if (user.TimeZoneId is not null) claims.Add(new Claim(JwtClaimTypes.zoneinfo, user.TimeZoneId));

            return claims;
        }

        private static Task OnRemoteFailure(RemoteFailureContext context)
        {
            // Services
            var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(CohubWebAppStartupExtensions));

            // Parse redirectUri and its query, or fallback to /session/login.
            var redirectUri = new Uri(new Uri(context.Request.Scheme + "://" + context.Request.Host.Value), context.Properties?.RedirectUri ?? "/session/login");
            var query = QueryHelpers.ParseQuery(redirectUri.Query);

            // Extract retries from the redirectUri
            int retry;
            if (query.TryGetValue("oidc-retry", out var value))
            {
                retry = int.Parse(value);
            }
            else
            {
                retry = 0;
            }

            if (retry < 5)
            {
                // Retry 5 times

                if (context.Failure is Exception ex)
                {
                    logger.LogWarning(ex, "OnRemoteFailure retry #{Retry}", retry);
                }
                else
                {
                    logger.LogWarning("OnRemoteFailure retry #{Retry}", retry);
                }

                query["oidc-retry"] = (retry + 1).ToString();
                string location = redirectUri.AbsolutePath + QueryString.Create(query).Value;

                context.Response.Redirect(location);
                context.HandleResponse();
            }
            else
            {
                // Give up after 5 retries

                if (context.Failure is Exception ex)
                {
                    logger.LogError(ex, "OnRemoteFailure gave up after retry #{Retry}", retry);
                }
                else
                {
                    logger.LogWarning("OnRemoteFailure gave up after retry #{Retry}", retry);
                }

                query.Remove("oidc-retry");
                string returnUrl = redirectUri.AbsolutePath + QueryString.Create(query).Value;
                string location = "/session/login-error" + QueryString.Create("returnUrl", returnUrl).Value;

                context.Response.Redirect(location);
                context.HandleResponse();
            }

            return Task.CompletedTask;
        }
    }
}
