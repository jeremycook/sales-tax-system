using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Period
    {
        public Period()
        {
        }

        public Period(DateTimeOffset created)
        {
            Created = created;
        }

        public Period(Period input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Period input)
        {
            FrequencyId = input.FrequencyId;
            Name = input.Name;
            DueDate = input.DueDate;
            StartDate = input.StartDate;
            EndDate = input.EndDate;
        }

        public override string ToString()
        {
            return Name;
        }

        [Required]
        public string Id { get; set; }

        [Required]
        [DataType("FrequencyId")]
        public string FrequencyId { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual Frequency Frequency { get; private set; }
        public virtual IReadOnlyList<Return> Returns { get; private set; }
        public virtual IReadOnlyList<StatementDue> StatementDetails { get; private set; }
        public virtual IReadOnlyList<TransactionDetail> TransactionDetails { get; private set; }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="DueDate"/> is in the past.
        /// <c><see cref="DueDate"/> &lt; <see cref="DateTime.Today"/></c>
        /// </summary>
        /// <returns></returns>
        public bool IsDue()
        {
            return DueDate < DateTime.Today;
        }

        /// <summary>
        /// Returns <c>true</c> if the <see cref="EndDate"/> is in the past.
        /// <c><see cref="EndDate"/> &lt; <see cref="DateTime.Today"/></c>
        /// </summary>
        /// <returns></returns>
        public bool IsPayable()
        {
            return EndDate < DateTime.Today;
        }
    }
}
