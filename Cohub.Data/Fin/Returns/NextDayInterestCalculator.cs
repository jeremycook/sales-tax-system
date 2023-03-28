using SiteKit.Extensions;
using System;

namespace Cohub.Data.Fin.Returns
{
    /// <summary>
    /// If you have September that is due 10/20/2020. On 10/21/2020 it should calculate interest for being one month late. If they still don't pay by 11/21/2020 then it would calculate interest for another month, and so on.
    /// </summary>
    public class NextDayInterestCalculator : IInterestCalculator
    {
        public decimal CalculateInterest(decimal interestPercent, DateTime dueDate, DateTime paymentDate, decimal netAmountDue)
        {
            if (paymentDate <= dueDate)
            {
                return 0;
            }

            var monthsPastDueDate = dueDate.AddDays(1).GetMonthsBetween(paymentDate) + 1;

            var interest = .01m * interestPercent * netAmountDue * monthsPastDueDate;

            return interest;
        }
    }
}
