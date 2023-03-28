namespace Cohub.WebApp.Areas.FileExplorer.Views.FileExplorer
{
    public class Edit
    {
        public string Id { get; set; } = null!;
        public string? ContentType { get; set; }

        public bool IsText { get; set; }
        public string? Content { get; set; }
    }
}
