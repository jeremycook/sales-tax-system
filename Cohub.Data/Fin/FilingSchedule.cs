using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    /// <summary>
    /// The <see cref="PaymentChart"/> that an <see cref="Organization"/>
    /// is required to follow over a range of time.
    /// </summary>
    public class FilingSchedule
    {
        public FilingSchedule()
        {
        }

        public FilingSchedule(DateTimeOffset created)
        {
            Created = created;
        }

        public FilingSchedule(FilingSchedule input)
        {
            OrganizationId ??= input.OrganizationId;
            UpdateWith(input);
        }

        public void UpdateWith(FilingSchedule input)
        {
            PaymentChartId = input.PaymentChartId;
            StartDate = input.StartDate;
            EndDate = input.EndDate;
        }

        public override string ToString()
        {
            return $"{OrganizationId} {PaymentChartId} {StartDate:d}-{EndDate:d}";
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string OrganizationId { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [Required]
        [DataType("PaymentChartId")]
        public int PaymentChartId { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual Org.Organization Organization { get; private set; }
        public virtual PaymentChart PaymentChart { get; private set; }
    }
}
