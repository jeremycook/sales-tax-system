using Microsoft.Extensions.Options;
using NodaTime;
using SiteKit.Jwt;
using System;
using System.Security.Claims;

namespace SiteKit.Info
{
    /// <summary>
    /// (Scoped) An actor represents the current user performing the action.
    /// Note that all properties are deterministic and set at the time
    /// this scoped service is instantiated.
    /// </summary>
    public class Actor
    {
        private Actor()
        {
            UserId = Users.StandardUserId.System;
            Name = "System";
            ZoneInfo = DateTimeZoneProviders.Tzdb["Etc/UTC"];
            ZonedNow = SystemClock.Instance.GetCurrentInstant().InZone(ZoneInfo);
            Now = ZonedNow.ToDateTimeOffset();
        }
        public Actor(IOptions<AboutOptions> about, ClaimsPrincipal user)
        {
            UserId = user.FindFirst(JwtClaimTypes.sub)?.Value is string sub && int.TryParse(sub, out int id) ?
                id :
                Users.StandardUserId.Anonymous;

            Name = user.FindFirst(JwtClaimTypes.name)?.Value is string name ?
                name :
                "Anonymous";

            Initials = user.FindFirst(JwtClaimTypes.initials)?.Value;

            ZoneInfo = user.FindFirst(JwtClaimTypes.zoneinfo)?.Value is string zoneinfo ?
                DateTimeZoneProviders.Tzdb[zoneinfo] :
                DateTimeZoneProviders.Tzdb[about.Value.Zoneinfo];

            ZonedNow = SystemClock.Instance.GetCurrentInstant().InZone(ZoneInfo);

            Now = ZonedNow.ToDateTimeOffset();
        }

        public static Actor System { get; } = new Actor();

        /// <summary>
        /// Returns the date/time in the <see cref="Actor"/>'s time zone.
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        public DateTimeOffset? LocalDateTimeOffset(DateTimeOffset? dateTimeOffset)
        {
            if (dateTimeOffset is null)
            {
                return null;
            }
            else
            {
                return OffsetDateTime.FromDateTimeOffset(dateTimeOffset.Value).InZone(ZoneInfo).ToDateTimeOffset();
            }
        }

        public int UserId { get; }
        public string Name { get; }
        public string? Initials { get; }
        public DateTimeZone ZoneInfo { get; }
        public ZonedDateTime ZonedNow { get; }
        public DateTimeOffset Now { get; }
    }
}
