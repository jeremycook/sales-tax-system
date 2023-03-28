using Cohub.Data.Fin.Deposits;
using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class StatementDue
    {
        public int Id { get; set; }

        public int StatementId { get; set; }

        [Required]
        public string CategoryId { get; set; }

        [Required]
        public string PeriodId { get; set; }

        public bool HasFiled { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal NetDue { get; set; }

        [DataType(DataType.Currency)]
        public decimal PenaltyDue { get; set; }

        [DataType(DataType.Currency)]
        public decimal InterestDue { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalDue { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalOverpayment { get; set; }

        public virtual Statement Statement { get; private set; }
    }
}
