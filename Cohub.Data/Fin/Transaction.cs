using SiteKit.ComponentModel;
using SiteKit.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Transaction
    {
        private decimal? _paymentAmount;
        private decimal? _balance;

        public Transaction()
        {
        }
        public Transaction(DateTimeOffset created)
        {
            Created = created;
        }
        public Transaction(Transaction input)
        {
            UpdateWith(input);
        }
        public void UpdateWith(Transaction input)
        {
            BatchId = input.BatchId;
            Note = input.Note;
        }

        public override string ToString()
        {
            return $"TX-{Id}";
        }

        public int Id { get; private set; }

        [Required]
        [DataType("BatchId")]
        public int BatchId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public virtual List<TransactionDetail> Details { get; set; }

        public DateTimeOffset Created { get; private set; }

        [DataType(DataType.Currency)]
        public decimal? Deposited { get => Details is null ? _paymentAmount : (_paymentAmount = Details.Where(o => o.BucketId == BucketId.Deposit).Sum(t => t.Amount)); private set => _paymentAmount = value; }

        [DataType(DataType.Currency)]
        [Display(Name = "Rev+Ovr")]
        public decimal? RevenueAndOverpayment
        {
            get => Details is null ? _balance : (_paymentAmount = Details.Where(t => t.BucketId == BucketId.Revenue || t.BucketId == BucketId.Overpayment).Sum(t => t.Amount));
            private set => _balance = value;
        }

        public virtual Batch Batch { get; private set; }
    }
}
