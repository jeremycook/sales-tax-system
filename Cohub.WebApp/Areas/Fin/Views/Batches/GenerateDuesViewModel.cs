using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Fin.Views.Batches
{
    public class GenerateDuesViewModel
    {
        [Required]
        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; } = DateTime.Today;

        [DataType("OrganizationIdList")]
        public List<string> OrganizationIds { get; set; } = new();
    }
}