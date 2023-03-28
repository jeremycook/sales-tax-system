using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class TaxFiling : Filing
    {
        public TaxFiling() : base(nameof(TaxFiling))
        {
        }

        public TaxFiling(DateTimeOffset created) : base(nameof(TaxFiling), created)
        {
        }

        [Required]
        [DataType(DataType.Currency)]
        [Display(Name = "Taxable Amount", Description = "Taxable sales and services.")]
        public decimal TaxableAmount { get; set; }

        [Required]
        [DataType(DataType.Currency)]
        public decimal ExcessTax { get; set; }

        public override void UpdateWith(Filing input)
        {
            UpdateWith((TaxFiling)input);
        }

        protected void UpdateWith(TaxFiling input)
        {
            FilingDate = input.FilingDate;
            TaxableAmount = input.TaxableAmount;
            ExcessTax = input.ExcessTax;
        }
    }
}
