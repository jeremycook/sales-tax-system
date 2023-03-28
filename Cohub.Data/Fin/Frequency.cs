using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin
{
    public class Frequency
    {
        public Frequency()
        {
        }

        public Frequency(DateTimeOffset created)
        {
            Created = created;
        }

        public Frequency(Frequency input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Frequency input)
        {
            IsActive = input.IsActive;
            Description = input.Description;
            PeriodIdFormat = input.PeriodIdFormat;
            PeriodNameFormat = input.PeriodNameFormat;
        }

        public override string ToString()
        {
            return Id;
        }

        [Required]
        public string Id { get; set; } = default!;

        public bool IsActive { get; set; }

        [DataType(DataType.MultilineText)]
        public string? Description { get; set; }

        public string? PeriodIdFormat { get; set; }

        public string? PeriodNameFormat { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual List<Period>? Periods { get; set; }
    }
}
