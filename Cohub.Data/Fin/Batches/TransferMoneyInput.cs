using SiteKit.ComponentModel;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin.Batches
{
    public class TransferMoneyInput
    {
        // Options:

        public TransferMoneyAction Action { get; set; }

        [DataType("BatchId")]
        public int? BatchId { get; set; }

        [DataType(DataType.Date)]
        [Display(Name = "New Effective Date", Description = "Leave blank to use the original effective date in all cases except when applying overpayments. When applying overpayments today's date will be used if this field is blank.")]
        public DateTime? NewEffectiveDate { get; set; }

        // Filters:

        [DataType("OrganizationId")]
        public string? OrganizationId { get; set; }

        [DataType("CategoryId")]
        public string? CategoryId { get; set; }

        [DataType("PeriodId")]
        public string? PeriodId { get; set; }

        [DataType("SubcategoryId")]
        public string? SubcategoryId { get; set; }

        public ReturnStatusId? ReturnStatusId { get; set; }

        [Boolean("Filed", "Unfiled", nullText: "")]
        [Display(Name = "Filing Status")]
        public bool? HasFiled { get; set; }

        [DataType(DataType.Currency)]
        public decimal? MinimumAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal? MaximumAmount { get; set; }
    }
}