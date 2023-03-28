using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Fin.Views.Batches
{
    public class RefundRequest
    {
        public RefundRequestCommand Command { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string? OrganizationId { get; set; }

        [DataType(DataType.Date)]
        public DateTime FromEffectiveDate { get; set; } = DateTime.Today.AddYears(-1);

        [DataType(DataType.Date)]
        public DateTime ThroughEffectiveDate { get; set; } = DateTime.Today;

        [DataType(DataType.Date)]
        public DateTime NewEffectiveDate { get; set; } = DateTime.Today;

        [DataType("RefundRequestAmountList")]
        public List<RefundRequestAmount>? Refunds { get; set; }
    }

    public enum RefundRequestCommand
    {
        Reset = 0,
        Refund,
    }

    public class RefundRequestAmount : IValidatableObject
    {
        [DataType("ReadOnly")]
        public string PeriodId { get; set; } = null!;

        [DataType("ReadOnly")]
        public string CategoryId { get; set; } = null!;

        [DataType("ReadOnly")]
        public string SubcategoryId { get; set; } = null!;

        [DataType("ReadOnly")]
        public string BucketId { get; set; } = null!;

        [DataType(DataType.Currency)]
        [Range(typeof(decimal), "0", "10000")]
        public decimal AvailableAmount { get; set; }

        [DataType(DataType.Currency)]
        [Range(typeof(decimal), "0", "10000")]
        public decimal RefundAmount { get; set; }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (RefundAmount > AvailableAmount)
            {
                yield return new ValidationResult("The Refund Amount cannot be greater than the Available Amount.", new[] { nameof(RefundAmount) });
            }
        }
    }
}
