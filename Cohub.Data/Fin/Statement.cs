using SiteKit.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Statement : IValidatableObject
    {
        public Statement()
        {
        }

        public override string ToString()
        {
            return $"{TypeId} #{Id} for {OrganizationId}";
        }

        [Editable(false)]
        public int Id { get; set; }

        public StatementTypeId TypeId { get; set; } = StatementTypeId.Statement;

        public StatementStatusId StatusId { get; set; } = StatementStatusId.Draft;

        [Required]
        public string OrganizationId { get; set; }

        [DataType(DataType.Date)]
        public DateTime NoticeDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? AssessmentDueDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal OverpaymentBalance { get; private set; }

        [DataType(DataType.Currency)]
        public decimal GrandTotalDue { get; private set; }

        [ScaffoldColumn(false)]
        public virtual List<StatementDue> Dues { get; set; }

        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Updated { get; private set; }

        public virtual Org.Organization Organization { get; private set; }
        public virtual IReadOnlyList<StatementComment> StatementComments { get; private set; }

        /// <summary>
        /// Recalculate the grand total.
        /// </summary>
        public void Recalculate()
        {
            GrandTotalDue = Dues.Sum(o => o.TotalDue);
            OverpaymentBalance = Dues.Sum(o => o.TotalOverpayment);
        }

        public void UpdateWith(Statement input)
        {
            TypeId = input.TypeId;
            StatusId = input.StatusId;
            OrganizationId = input.OrganizationId;
            if (NoticeDate != input.NoticeDate)
            {
                NoticeDate = input.NoticeDate;
                Dues?.ForEach(d => d.DueDate = input.NoticeDate);
            }
            AssessmentDueDate = input.AssessmentDueDate;
        }

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (TypeId == StatementTypeId.Assessment &&
                AssessmentDueDate == null)
            {
                yield return new ValidationResult("The Assessment Due Date field must be set for assessments.", new[] { nameof(AssessmentDueDate) });
            }

            if (AssessmentDueDate < NoticeDate)
            {
                yield return new ValidationResult("The Assessment Due Date cannot come before the Notice Date.", new[] { nameof(AssessmentDueDate) });
            }
        }
    }
}
