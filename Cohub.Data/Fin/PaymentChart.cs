using Cohub.Data.Usr;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class PaymentChart
    {
        public PaymentChart()
        {
        }

        public PaymentChart(DateTimeOffset created)
        {
            Created = created;
        }

        public PaymentChart(PaymentChart input)
        {
            UpdateWith(input);
        }

        public void UpdateWith(PaymentChart input)
        {
            IsActive = input.IsActive;
            Name = input.Name;
            CategoryId = input.CategoryId;
            FrequencyId = input.FrequencyId;
            Description = input.Description;
        }

        public override string ToString()
        {
            return Name;
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        [DataType("CategoryId")]
        public string CategoryId { get; set; }

        [Required]
        [DataType("FrequencyId")]
        public string FrequencyId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }
        public int CreatedById { get; private set; }

        public DateTimeOffset Updated { get; private set; }
        public int UpdatedById { get; private set; }

        [ScaffoldColumn(false)]
        public virtual List<PaymentConfiguration> Configurations { get; set; }

        public virtual Frequency Frequency { get; private set; }
        public virtual Category Category { get; private set; }
        public virtual User CreatedBy { get; private set; }
        public virtual User UpdatedBy { get; private set; }
        public virtual IReadOnlyList<FilingSchedule> FilingSchedules { get; private set; }
    }
}
