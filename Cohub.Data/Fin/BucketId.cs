using System.Collections.Generic;

namespace Cohub.Data.Fin
{
    public static class BucketId
    {
        public const string Deposit = "Dep";

        public const string Revenue = "Rev";
        public const string Overpayment = "Ovr";

        public const string Adjustment = "Adj";

        public const string Due = "Due";

        /// <summary>
        /// <c>Revenue + Overpayment</c> should equal the balance of the Deposit bucket.
        /// </summary>
        public static IReadOnlyList<string> TotalDeposited { get; } = new[]
        {
            Revenue, Overpayment
        };

        /// <summary>
        /// <c>Revenue + Due + Adjustment</c> should equal the total due on the effective date.
        /// </summary>
        public static IReadOnlyList<string> TotalDue { get; } = new[]
        {
            Revenue, Due, Adjustment
        };
    }
}
