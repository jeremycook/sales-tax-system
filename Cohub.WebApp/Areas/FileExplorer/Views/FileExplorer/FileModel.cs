using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.FileExplorer.Views.FileExplorer
{
    [Display(Name = "File", GroupName = "Files")]
    public class FileModel
    {
        public string Id { get; set; } = null!;
        public string Subpath { get; set; } = null!;
        public bool IsText { get; set; }
        public string ContentType { get; set; } = null!;
        public bool Exists { get; set; }
        public bool IsDirectory { get; set; }
        public string PhysicalPath { get; set; } = null!;
        public bool IsTrusted { get; set; }

        public override string ToString()
        {
            return Subpath;
        }
    }
}
