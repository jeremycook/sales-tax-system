using Cohub.Data.Fin;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Fin.Views.Periods
{
    public class CreatePeriod
    {
        [Required]
        [DataType("FrequencyId")]
        public string FrequencyId { get; set; } = string.Empty;

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [Display(Description = "Leave blank to auto generate.")]
        public string? Id { get; set; }

        [Display(Description = "Leave blank to auto generate.")]
        public string? Name { get; set; }

        public bool CreateAnother { get; set; }

        public Period Create(Frequency frequency)
        {
            var period = new Period
            {
                Id = Id,
                Name = Name,
                FrequencyId = FrequencyId,
                StartDate = StartDate,
                EndDate = EndDate,
                DueDate = DueDate
            };

            if (frequency.PeriodIdFormat != null)
                period.Id ??= string.Format(frequency.PeriodIdFormat ?? (frequency.Id + " {0:yyyy-MM-dd}"), period.StartDate, period.EndDate, period.DueDate, (period.StartDate.Month + 2) / 3, (period.EndDate.Month + 2) / 3, (period.DueDate.Month + 2) / 3);

            if (frequency.PeriodNameFormat != null)
                period.Name ??= string.Format(frequency.PeriodNameFormat ?? (frequency.Id + " {0:yyyy-MM-dd}"), period.StartDate, period.EndDate, period.DueDate, (period.StartDate.Month + 2) / 3, (period.EndDate.Month + 2) / 3, (period.DueDate.Month + 2) / 3);

            return period;
        }
    }
}
