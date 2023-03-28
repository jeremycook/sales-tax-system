using System;

namespace Cohub.Data.Fin.Returns
{
    public interface IInterestCalculator
    {
        decimal CalculateInterest(decimal interestPercent, DateTime dueDate, DateTime paymentDate, decimal netAmountDue);
    }
}
