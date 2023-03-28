using System.Collections.Generic;
using System.Linq;

namespace System.Security.Claims
{
    public static class UsrClaimsPrincipalExtensions
    {
        public static bool InAnyRole(this ClaimsPrincipal claimsPrincipal, IEnumerable<string> roles)
        {
            return roles.Any(role => claimsPrincipal.IsInRole(role));
        }

        public static bool IsSuper(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.InAnyRole(Cohub.Data.Usr.Policy.Roles[Cohub.Data.Usr.Policy.Super]);
        }

        public static bool CanManage(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.InAnyRole(Cohub.Data.Usr.Policy.Roles[Cohub.Data.Usr.Policy.Manage]);
        }

        public static bool CanProcess(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.InAnyRole(Cohub.Data.Usr.Policy.Roles[Cohub.Data.Usr.Policy.Process]);
        }

        public static bool CanAudit(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.InAnyRole(Cohub.Data.Usr.Policy.Roles[Cohub.Data.Usr.Policy.Audit]);
        }

        public static bool CanReview(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.InAnyRole(Cohub.Data.Usr.Policy.Roles[Cohub.Data.Usr.Policy.Review]);
        }

        public static bool CanReviewLicenses(this ClaimsPrincipal claimsPrincipal)
        {
            return claimsPrincipal.InAnyRole(Cohub.Data.Usr.Policy.Roles[Cohub.Data.Usr.Policy.ReviewLicenses]);
        }
    }
}
