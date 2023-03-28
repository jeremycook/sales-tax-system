using SiteKit.Jwt;

namespace System.Security.Claims
{
    public static class SiteKitClaimPrincipalExtensions
    {
        public static string? Sub(this ClaimsPrincipal principal)
        {
            return principal.FindFirst(JwtClaimTypes.sub)?.Value is string sub ?
                sub :
                null;
        }
    }
}
