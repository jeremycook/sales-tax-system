using System;
using System.Security.Claims;

namespace SiteKit.Jwt
{
    public static class JwtClaimTypes
    {
        public const string email = "email";
        public const string email_verified = "email_verified";
        public const string initials = "initials";
        public const string iss = "iss";
        public const string locale = "locale";
        public const string name = "name";
        public const string phone_number = "phone_number";
        public const string phone_number_verified = "phone_number_verified";
        public const string preferred_username = "preferred_username";
        public const string role = "role";
        public const string sub = "sub";
        public const string username = "username";
        public const string zoneinfo = "zoneinfo";
    }
}
