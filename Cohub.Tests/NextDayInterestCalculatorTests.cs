using Cohub.Data.Fin.Returns;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Cohub.Tests
{
    /// <summary>
    /// If you have September that is due 10/20/2020. On 10/21/2020 it should calculate interest for being one month late. If they still don't pay by 11/21/2020 then it would calculate interest for another month, and so on.
    /// </summary>
    [TestClass]
    public class NextDayInterestCalculatorTests
    {
        readonly NextDayInterestCalculator calculator = new NextDayInterestCalculator();

        [TestMethod]
        public void PayOnDueDate()
        {
            Assert.AreEqual(0, calculator.CalculateInterest(1, dueDate: new DateTime(2020, 10, 20), paymentDate: new DateTime(2020, 10, 20), netAmountDue: 1000));
        }

        [TestMethod]
        public void PayDayAfterDueDate()
        {
            Assert.AreEqual(10, calculator.CalculateInterest(1, dueDate: new DateTime(2020, 10, 20), paymentDate: new DateTime(2020, 10, 21), netAmountDue: 1000));
        }

        [TestMethod]
        public void Pay1MonthFromDueDate()
        {
            Assert.AreEqual(10, calculator.CalculateInterest(1, dueDate: new DateTime(2020, 10, 20), paymentDate: new DateTime(2020, 11, 20), netAmountDue: 1000));
        }

        [TestMethod]
        public void Pay1MonthAfterDueDate()
        {
            Assert.AreEqual(20, calculator.CalculateInterest(1, dueDate: new DateTime(2020, 10, 20), paymentDate: new DateTime(2020, 11, 21), netAmountDue: 1000));
        }

        [TestMethod]
        public void Pay2MonthFromDueDate()
        {
            Assert.AreEqual(20, calculator.CalculateInterest(1, dueDate: new DateTime(2020, 10, 20), paymentDate: new DateTime(2020, 12, 20), netAmountDue: 1000));
        }

        [TestMethod]
        public void Pay2MonthAfterDueDate()
        {
            Assert.AreEqual(30, calculator.CalculateInterest(1, dueDate: new DateTime(2020, 10, 20), paymentDate: new DateTime(2020, 12, 21), netAmountDue: 1000));
        }
    }
}
