using SiteKit.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class PaymentConfiguration
    {
        public PaymentConfiguration()
        {
        }

        public PaymentConfiguration(DateTimeOffset created)
        {
            Created = created;
        }

        public PaymentConfiguration(PaymentConfiguration input)
        {
            PaymentChartId = input.PaymentChartId;
            UpdateWith(input);
        }

        public void UpdateWith(PaymentConfiguration input)
        {
            PaymentChartId = input.PaymentChartId;
            StartDate = input.StartDate;
            EndDate = input.EndDate;
            VendorFeePercentage = input.VendorFeePercentage;
            VendorFeeMax = input.VendorFeeMax;
            PenaltyPercentage = input.PenaltyPercentage;
            InterestPercentage = input.InterestPercentage;
            TaxPercentage = input.TaxPercentage;
            EstimatedNetAmountDuePercentage = input.EstimatedNetAmountDuePercentage;
            MinimumEstimatedNetAmountDue = input.MinimumEstimatedNetAmountDue;
        }

        /// <summary>
        /// Estimate net amount due based on <paramref name="historicNetAmountDue"/>,
        /// <see cref="EstimatedNetAmountDuePercentage"/> and
        /// <see cref="MinimumEstimatedNetAmountDue"/>.
        /// </summary>
        /// <param name="historicNetAmountDue"></param>
        /// <param name="paymentConfiguration"></param>
        /// <returns></returns>
        public decimal EstimateNetAmountDue(decimal? historicNetAmountDue)
        {
            decimal historicEstimate = 0.01m * EstimatedNetAmountDuePercentage * (historicNetAmountDue ?? 0);
            return Math.Max(MinimumEstimatedNetAmountDue, historicEstimate.RoundAwayFromZero());
        }

        public override string ToString()
        {
            return Id > 0 ? $"#{Id}" : "New";
        }

        [Editable(false)]
        public int Id { get; set; }

        [DataType("PaymentChartId")]
        public int PaymentChartId { get; set; }

        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; } = DateTime.MinValue.Date;

        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; } = DateTime.MaxValue.Date;

        public decimal VendorFeePercentage { get; set; }

        [DataType(DataType.Currency)]
        public decimal VendorFeeMax { get; set; }

        public decimal PenaltyPercentage { get; set; }

        public decimal InterestPercentage { get; set; }

        /// <summary>
        /// If this is configuration for tax returns this value will be used
        /// when calculating return's net amount due (a.k.a. net tax due) based
        /// on the return's taxable amount.
        /// </summary>
        [Display(Name = "Tax Percentage", Description = "If this is configuration for tax returns this value will be used when calculating return's net amount due (a.k.a. net tax due) based on the return's taxable amount.")]
        public decimal TaxPercentage { get; set; }

        /// <summary>
        /// If net amount due is estimated for a return, this percentage will be
        /// multiplied by the historic value to calculate the estimated net 
        /// tax due. Setting this to 150% will result in the estimated amount
        /// being 150% of the historic amount.
        /// </summary>
        [Display(Name = "Estimated Net Amount Due Percentage", Description = "If net amount due is estimated for a return, this percentage will be multiplied by the historic value to calculate the estimated net tax due. Setting this to 150% will result in the estimated amount being 150% of the historic amount.")]
        public decimal EstimatedNetAmountDuePercentage { get; set; }

        /// <summary>
        /// If net amount due is estimated for a return, this value will be
        /// used if the amount cannot be estimated based on historic values or
        /// if the historic estimate is less than this value.
        /// </summary>
        [DataType(DataType.Currency)]
        [Display(Name = "Minimum Estimated Net Amount Due", Description = "If net amount due is estimated for a return, this value will be used if the amount cannot be estimated based on historic values or if the historic estimate is less than this value.")]
        public decimal MinimumEstimatedNetAmountDue { get; set; }

        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Updated { get; private set; }

        public virtual PaymentChart PaymentChart { get; private set; }
    }
}