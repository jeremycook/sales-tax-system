using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class FeeFiling : Filing
    {
        public FeeFiling() : base(nameof(FeeFiling))
        {
        }

        public FeeFiling(DateTimeOffset created) : base(nameof(FeeFiling), created)
        {
        }

        public override void UpdateWith(Filing input)
        {
            UpdateWith((FeeFiling)input);
        }

        protected void UpdateWith(FeeFiling input)
        {
            FilingDate = input.FilingDate;
            FeeAmount = input.FeeAmount;
        }

        [Required]
        [DataType(DataType.Currency)]
        public decimal FeeAmount { get; set; }
    }
}
