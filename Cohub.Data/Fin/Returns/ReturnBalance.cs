using Cohub.Data.Org;
using SiteKit.Extensions;
using System;

namespace Cohub.Data.Fin.Returns
{
    public class ReturnBalance
    {
        public Return Return { get; set; } = null!;
        public Organization Organization { get; set; } = null!;
        public Category Category { get; set; } = null!;
        public Period Period { get; set; } = null!;
        public Filing? LastFiling { get; set; }
        public PaymentConfiguration PaymentConfiguration { get; set; } = null!;
        public DateTime? MostRecentDateRevOrAdjEffectiveDate { get; set; }
        public decimal? HistoricNetAmountDue { get; set; }
        public decimal Overpayment { get; set; }
        public decimal OnTimeNetRevAndAdj { get; set; }
        public decimal NetRevenue { get; set; }
        public decimal NetDue { get; set; }
        public decimal NetAdjustment { get; set; }
        public decimal PenaltyRevenue { get; set; }
        public decimal PenaltyDue { get; set; }
        public decimal PenaltyAdjustment { get; set; }
        public decimal InterestRevenue { get; set; }
        public decimal InterestDue { get; set; }
        public decimal InterestAdjustment { get; set; }

        public decimal TotalNetRevAndAdj => NetRevenue.RoundAwayFromZero() + NetAdjustment.RoundAwayFromZero();
        public decimal TotalPenaltyRevAndAdj => PenaltyRevenue.RoundAwayFromZero() + PenaltyAdjustment.RoundAwayFromZero();
        public decimal TotalInterestRevAndAdj => InterestRevenue.RoundAwayFromZero() + InterestAdjustment.RoundAwayFromZero();

    }
}
