using Cohub.Data.Org;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin.Deposits
{
    public class DepositPayment : IValidatableObject
    {
        [Required]
        [DataType("OrganizationId")]
        public string? OrganizationId { get; set; }

        [Required]
        [DataType("CategoryId")]
        public string? CategoryId { get; set; }

        [Required]
        [DataType("PeriodId")]
        public string? PeriodId { get; set; }

        [DataType("ReturnId")]
        public int? ReturnId { get; set; }

        [Required]
        [Range(typeof(decimal), "0", "100000000")]
        [DataType(DataType.Currency)]
        public decimal? PaymentAmount { get; set; }

        [Range(typeof(decimal), "0", "100000000")]
        [DataType(DataType.Currency)]
        public decimal? Taxable { get; set; }

        [Range(typeof(decimal), "0", "100000000")]
        [DataType(DataType.Currency)]
        public decimal? Excess { get; set; }

        [Range(typeof(decimal), "0", "100000000")]
        [DataType(DataType.Currency)]
        public decimal? Assessment { get; set; }

        [Range(typeof(decimal), "0", "100000000")]
        [DataType(DataType.Currency)]
        public decimal? Fees { get; set; }

        // Read only information

        public PaymentSnapshot Snapshot { get; set; } = Activator.CreateInstance<PaymentSnapshot>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var db = (CohubDbContext)validationContext.GetService(typeof(CohubDbContext))!;

            if (db.Organizations().Find(OrganizationId) == null)
            {
                yield return new ValidationResult($"The {OrganizationId} Organization ID was not found.", new[] { nameof(OrganizationId) });
            }

            if (db.Categories().Find(CategoryId) == null)
            {
                yield return new ValidationResult($"The {CategoryId} Category ID was not found.", new[] { nameof(CategoryId) });
            }

            if (db.Periods().Find(PeriodId) == null)
            {
                yield return new ValidationResult($"The {PeriodId} Period ID was not found.", new[] { nameof(PeriodId) });
            }
        }
    }
}