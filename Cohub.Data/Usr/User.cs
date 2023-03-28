using SiteKit.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Usr
{
    public class User
    {
        public User()
        {
        }
        public User(DateTimeOffset created)
        {
            Created = created;
            RoleId = Usr.RoleId.Disabled;
        }

        /// <summary>
        /// Sets <see cref="IsActive"/> to <c>false</c>.
        /// </summary>
        public void Disable()
        {
            IsActive = false;
        }

        public override string ToString()
        {
            return Username;
        }

        public int Id { get; set; }

        [Boolean("Active", "Disabled")]
        public bool IsActive { get; set; }

        [Required]
        public string Username { get; set; } = null!;

        /// <summary>
        /// Display name
        /// </summary>
        [Required]
        public string Name { get; set; } = null!;
        public string? Initials { get; set; }
        public string? Email { get; set; }
        [Required]
        public string RoleId { get; set; } = null!;
        [ScaffoldColumn(false)]
        public string? TimeZoneId { get; set; }
        [ScaffoldColumn(false)]
        public string? LocaleId { get; set; }

        public virtual List<UserLogin> Logins { get; set; } = new();

        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Updated { get; private set; }

        [Required]
        [ScaffoldColumn(false)]
        public string LowercaseUsername { get => Username.ToLowerInvariant(); private set { } }

        public virtual Role? Role { get; private set; }
        public virtual Geo.Locale? Locale { get; private set; }
        public virtual Geo.Tz? TimeZone { get; private set; }
        public virtual IReadOnlyList<Comment>? Comments { get; set; }
        public virtual IReadOnlyList<Document>? Documents { get; set; }
        public virtual IReadOnlyList<UserMention>? UserMentions { get; set; }
    }
}
