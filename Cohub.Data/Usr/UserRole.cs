using System;

#nullable disable

namespace Cohub.Data.Usr
{
    public class UserRole
    {
        public int UserId { get; set; }
        public string RoleId { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual Role Role { get; private set; }
        public virtual User User { get; private set; }
    }
}
