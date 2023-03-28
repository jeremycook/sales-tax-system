using Cohub.Data.Org;

#nullable disable

namespace Cohub.Data.Fin.Returns
{
    public class ReturnSummary
    {
        public virtual Return Return { get; set; }
        public virtual ReturnStatus Status { get; set; }
        public virtual Organization Organization { get; set; }
        public virtual Period Period { get; set; }
        public virtual Category Category { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal NetDue { get; set; }
        public decimal PenaltyDue { get; set; }
        public decimal InterestDue { get; set; }
        /// <summary>
        /// Balance of the <see cref="CategoryId.NSFFee"/> category for the organization and period.
        /// </summary>
        public decimal NSFFee { get; set; }
        /// <summary>
        /// Balance of the <see cref="BucketId.Overpayment"/> bucket for the organization, period and category.
        /// </summary>
        public decimal Ovr { get; set; }
    }
}
