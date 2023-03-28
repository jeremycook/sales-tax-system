using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class AssessmentFiling : Filing
    {
        public AssessmentFiling() : base(nameof(AssessmentFiling))
        {
        }

        public AssessmentFiling(DateTimeOffset created) : base(nameof(AssessmentFiling), created)
        {
        }

        public override void UpdateWith(Filing input)
        {
            UpdateWith((AssessmentFiling)input);
        }

        protected void UpdateWith(AssessmentFiling input)
        {
            AssessmentAmount = input.AssessmentAmount;
            FilingDate = input.FilingDate;
        }

        [Required]
        [DataType(DataType.Currency)]
        public decimal AssessmentAmount { get; set; }
    }
}
