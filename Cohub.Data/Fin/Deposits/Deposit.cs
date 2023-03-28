using Cohub.Data.Org;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin.Deposits
{
    public class Deposit : IValidatableObject
    {
        private DateTime? _DepositDate;

        [Required]
        [DataType("OrganizationId")]
        public string? DepositorId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime? DepositDate
        {
            get => _DepositDate; set
            {
                if (value == null)
                {
                    _DepositDate = null;
                }
                else if (value.Value.TimeOfDay == TimeSpan.Zero)
                {
                    _DepositDate = value;
                }
                else
                {
                    var localDateTime = value.Value.ToLocalTime();
                    if (localDateTime.TimeOfDay != TimeSpan.Zero)
                    {
                        throw new ArgumentException($"The {nameof(DepositDate)} must be a date with no time element. The original value was {value} and kind {value.Value.Kind}, and {localDateTime} when converted to local DateTime.", nameof(value));
                    }
                    _DepositDate = localDateTime;
                }
            }
        }

        [Required]
        [DataType(DataType.Currency)]
        public decimal? DepositAmount { get; set; }

        public List<DepositPayment> Payments { get; set; } = new();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var db = (CohubDbContext)validationContext.GetService(typeof(CohubDbContext))!;

            if (db.Organizations().Find(DepositorId) == null)
            {
                yield return new ValidationResult($"The {DepositorId} Depositor ID was not found.", new[] { nameof(DepositorId) });
            }
        }
    }
}
