using Cohub.Data.Org;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Org.Views.Organizations
{
    public class CloseOrganization
    {
#nullable disable // Work MVC model binder treating this like required
        public Organization Organization { get; internal set; }
#nullable enable

        [Required]
        [DataType(DataType.Date)]
        public DateTime ClosedDate { get; set; } = DateTime.Today;
    }
}
