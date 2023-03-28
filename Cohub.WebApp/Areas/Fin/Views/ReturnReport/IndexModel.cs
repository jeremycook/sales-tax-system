using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Fin.Views.ReturnReport
{
    public class IndexModel
    {
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = new DateTime(DateTime.Today.Year, 12, 31);

        [DataType("OrganizationIdList")]
        [Display(Name = "Organizations", Description = "Leave blank to print returns of all organizations with Send Physical Mail set. Select one or more organizations to print returns for them, ignoring the Send Physical Mail flag.")]
        public List<string> OrganizationIds { get; set; } = new List<string>();

        [DataType("CategoryIdList")]
        [Display(Name = "Categories")]
        public List<string> CategoryIds { get; set; } = new List<string>();
    }
}
