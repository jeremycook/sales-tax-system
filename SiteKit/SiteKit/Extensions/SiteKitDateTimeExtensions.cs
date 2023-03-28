using System;

namespace SiteKit.Extensions
{
    public static class SiteKitDateTimeExtensions
    {
        /// <summary>
        /// Calculates the number of months between <paramref name="thisDate"/> and <paramref name="thatDate"/>.
        /// The result is always positive.
        /// </summary>
        /// <param name="thisDate"></param>
        /// <param name="thatDate"></param>
        /// <returns></returns>
        public static int GetMonthsBetween(this DateTime thisDate, DateTime thatDate)
        {
            if (thisDate > thatDate) return GetMonthsBetween(thatDate, thisDate);

            var monthDiff = Math.Abs((thatDate.Year * 12 + (thatDate.Month - 1)) - (thisDate.Year * 12 + (thisDate.Month - 1)));

            if (thisDate.AddMonths(monthDiff) > thatDate || thatDate.Day < thisDate.Day)
            {
                return monthDiff - 1;
            }
            else
            {
                return monthDiff;
            }
        }

        public static bool DateIsInTheFuture(this DateTime date)
        {
            return date > DateTime.Today;
        }
    }
}
