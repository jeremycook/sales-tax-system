using SiteKit.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin.Deposits
{
    public class PaymentSnapshot
    {
        [Obsolete("runtime")]
        public PaymentSnapshot()
        {
        }

        public PaymentSnapshot(DateTime paymentDate, DateTime dueDate, bool hasFiling, decimal vendorFee, decimal netAmount, decimal netDue, decimal penaltyDue, decimal interestDue)
        {
            PaymentDate = paymentDate;
            DueDate = dueDate;
            PaidOnTime = paymentDate <= dueDate;
            HasFiling = hasFiling;
            VendorFee = vendorFee.RoundAwayFromZero();
            NetAmount = netAmount.RoundAwayFromZero();
            NetDue = netDue.RoundAwayFromZero();
            PenaltyDue = penaltyDue.RoundAwayFromZero();
            InterestDue = interestDue.RoundAwayFromZero();
            TotalDue = NetDue + PenaltyDue + InterestDue;
        }

        public static PaymentSnapshot CreateEmptySnapshot()
        {
            return Activator.CreateInstance<PaymentSnapshot>();
        }

        [DataType(DataType.Date)]
        public DateTime PaymentDate { get; }
        [DataType(DataType.Date)]
        public DateTime DueDate { get; }
        public bool PaidOnTime { get; }
        public bool HasFiling { get;  }
        [DataType(DataType.Currency)]
        public decimal VendorFee { get; }
        /// <summary>
        /// The net amount to paid before penalty and interest.
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal NetAmount { get; }
        /// <summary>
        /// The net amount less what has been paid.
        /// </summary>
        [DataType(DataType.Currency)]
        public decimal NetDue { get; }
        [DataType(DataType.Currency)]
        public decimal PenaltyDue { get; }
        [DataType(DataType.Currency)]
        public decimal InterestDue { get; }
        [DataType(DataType.Currency)]
        public decimal TotalDue { get; }
    }
}
