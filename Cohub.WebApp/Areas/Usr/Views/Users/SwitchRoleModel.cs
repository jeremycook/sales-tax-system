using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Usr.Views.Users
{
    public class SwitchRoleModel
    {
        [DataType("SwitchRoleId")]
        [Required]
        public string? Role { get; set; }
    }
}
