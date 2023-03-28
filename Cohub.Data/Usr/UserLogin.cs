using System;

#nullable disable

namespace Cohub.Data.Usr
{
    public class UserLogin
    {
        public override string ToString()
        {
            return Issuer;
        }

        public int UserId { get; set; }
        public string Issuer { get; set; }
        public string Sub { get; set; }
        public DateTimeOffset? ExpiresAt { get; set; }
        public DateTimeOffset Created { get; private set; }

        public virtual User User { get; set; }
    }
}
