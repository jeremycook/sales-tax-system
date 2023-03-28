namespace Cohub.WebApp.Areas.Usr.Views.Users
{
    public class AccessModel
    {
        public string? Area { get; init; } = null!;
        public string Controller { get; init; } = null!;
        public string Action { get; init; } = null!;
        public string Method { get; init; } = null!;
        public string[] Roles { get; init; } = null!;
        public bool AllowAnonymous { get; init; }
    }
}
