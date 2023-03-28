using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Org.Views.Organizations
{
    public class ChangeId
    {
        [Required]
        [Display(Name = "New Organization ID")]
        public string NewOrganizationId { get; set; } = null!;
    }
}
