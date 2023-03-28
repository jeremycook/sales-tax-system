using System;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Fin.Views.Statements
{
    public class GenerateStatements
    {
        [DataType(DataType.Date)]
        public DateTime NoticeDate { get; set; } = DateTime.Today;

        [Display(Name = "Max End Date", Description = "Only transactions with a period End Date that is on or before this date will be considered.")]
        [DataType(DataType.Date)]
        public DateTime MaxEndDate { get; set; } = DateTime.Today;

        public bool ReplaceMatchingDrafts { get; set; } = true;

        [Required]
        [Display(Name = "Organization", Description = "Enter \"*\" to search all organizations.")]
        public string OrganizationId { get; set; } = null!;

        [DataType(DataType.Currency)]
        public decimal MinimumOverpayment { get; set; } = 10;
    }
}
