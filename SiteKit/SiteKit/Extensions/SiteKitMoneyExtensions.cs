using System;

namespace SiteKit.Extensions
{
    public static class SiteKitMoneyExtensions
    {
        private const decimal halfCentM = 0.005m;

        /// <summary>
        /// Return <c>true</c> if <paramref name="dollarAmount"/> is less than a negative half-cent.
        /// </summary>
        /// <param name="dollarAmount"></param>
        /// <returns></returns>
        public static bool IsNegativeOneCentOrLess(this decimal dollarAmount)
        {
            return dollarAmount < -halfCentM;
        }

        /// <summary>
        /// Return <c>true</c> if <paramref name="dollarAmount"/> is greater than or equal to a half-cent.
        /// </summary>
        /// <param name="dollarAmount"></param>
        /// <returns></returns>
        public static bool IsOneCentOrMore(this decimal dollarAmount)
        {
            return dollarAmount >= halfCentM;
        }

        /// <summary>
        /// Return <c>true</c> if <paramref name="dollarAmount"/> is within a half-cent of 0.
        /// </summary>
        /// <param name="dollarAmount"></param>
        /// <returns></returns>
        public static bool IsZeroCents(this decimal? dollarAmount)
        {
            return dollarAmount != null && dollarAmount.Value.IsZeroCents();
        }

        /// <summary>
        /// Return <c>true</c> if <paramref name="dollarAmount"/> is within a half-cent of 0.
        /// </summary>
        /// <param name="dollarAmount"></param>
        /// <returns></returns>
        public static bool IsZeroCents(this decimal dollarAmount)
        {
            return Math.Abs(dollarAmount) < halfCentM;
        }

        /// <summary>
        /// Rounds the <paramref name="amount"/> to <paramref name="decimals"/>
        /// away from <c>0</c>. For example: -1.5 when rounded to 0 decimals will round to -2 instead of -1.
        /// This matches the rounding behavior of the PostgreSQL <c>numeric</c> types
        /// (<see cref="https://www.postgresql.org/docs/current/datatype-numeric.html#DATATYPE-NUMERIC-DECIMAL"/>).
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public static decimal RoundAwayFromZero(this decimal amount, int decimals = 2)
        {
            return amount >= 0 ?
                Math.Round(Math.Abs(amount), decimals) :
                -Math.Round(Math.Abs(amount), decimals);
        }
    }
}
