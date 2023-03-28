using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SiteKit.DependencyInjection;
using SiteKit.Info;
using SiteKit.Jwt;
using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Cohub.Data.Usr
{
    [Scoped]
    public class UserRepository
    {
        private static readonly Random random = new Random();

        private readonly ILogger<UserRepository> logger;
        private readonly Actor actor;
        private readonly IOptions<AboutOptions> about;
        private readonly CohubDbContext db;

        public UserRepository(ILogger<UserRepository> logger, Actor actor, IOptions<AboutOptions> about, CohubDbContext db)
        {
            this.logger = logger;
            this.actor = actor;
            this.about = about;
            this.db = db;
        }

        public IQueryable<User> Query => db.Set<User>()
            .Include(o => o.Role)
            .Include(o => o.Logins);

        /// <summary>
        /// Tries to match a <see cref="User"/> based on <see cref="UserLogin"/>,
        /// otherwise tries to match based on <see cref="User.Email"/> and then binds by creating a new <see cref="UserLogin"/>.
        /// Returns <c>null</c> if a matching user was not found.
        /// </summary>
        /// <param name="principal"></param>
        /// <returns></returns>
        public virtual async Task<User?> FindByLoginOrBindByEmailAsync(ClaimsPrincipal principal)
        {
            var issuer =
                principal.FindFirst(JwtClaimTypes.iss)?.Value ??
                throw new ArgumentException($"Missing '{JwtClaimTypes.iss}' claim.", nameof(principal));

            string sub =
                principal.FindFirst(JwtClaimTypes.sub)?.Value ??
                principal.FindFirst(ClaimTypes.NameIdentifier)?.Value ??
                throw new ArgumentException($"Missing '{JwtClaimTypes.sub}' or '{ClaimTypes.NameIdentifier}' claim.", nameof(principal));

            var email = (
                principal.FindFirst(JwtClaimTypes.email)?.Value ??
                principal.FindFirst(ClaimTypes.Email)?.Value
            )?.ToLowerInvariant();

            if (!bool.TryParse(principal.FindFirst("email_verified")?.Value ?? bool.FalseString, out bool emailVerified))
            {
                // Parsing failed, assume false
                emailVerified = false;
            }

            // First try to match based on issuer and sub.
            var user = await Query
                .SingleOrDefaultAsync(o => o.IsActive && o.RoleId != RoleId.Disabled && o.Logins.Any(l => l.Issuer == issuer && l.Sub == sub));

            // If a user was not found and a verified email was provided try to match on email
            if (user == null && email != null && emailVerified)
            {
                // Try to match exactly one email address that has not already been bound to this provider
                var users = await Query
                    .Where(o => o.IsActive && o.RoleId != RoleId.Disabled && o.Email == email && !o.Logins.Any(l => l.Issuer == issuer))
                    .ToListAsync();
                if (users.Count == 1)
                {
                    user = users[0];

                    // Bind to the provider
                    db.Add(new UserLogin { UserId = user.Id, Issuer = issuer, Sub = sub });
                    db.Comment($"Added {issuer} login to {user.Username} {user.Id} user based on {email} email.");

                    await db.SaveChangesAsync();

                    logger.LogInformation("Bound {EntityType} {Id} by {UserId} based on email", nameof(User), user.Id, actor.UserId);
                }
            }

            return user;
        }
    }
}
