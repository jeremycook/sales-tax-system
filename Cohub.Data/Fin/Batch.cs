using SiteKit.ComponentModel;
using SiteKit.Extensions;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Batch
    {
        private decimal? _totalDeposited;
        private decimal? _totalRevenueAndOverpayment;

        public Batch()
        {
            IsPosted = false;
        }

        public Batch(Batch input)
        {
            IsPosted = false;
            UpdateWith(input);
        }

        public Batch(DateTimeOffset created, int createdById)
        {
            IsPosted = false;
            Created = created;
            CreatedById = createdById;
        }

        public void UpdateWith(Batch input)
        {
            Name = input.Name;
            DepositControlAmount = input.DepositControlAmount;
            Note = input.Note;
        }

        public override string ToString()
        {
            return Name;
        }

        public int Id { get; set; }

        [Boolean("Posted", "Unposted")]
        public bool IsPosted { get; private set; }

        [Boolean("Balanced", "Unbalanced")]
        public bool IsBalanced
        {
            get => TotalDeposited is null || TotalRevenueAndOverpayment is null ? false : (DepositControlAmount - TotalDeposited.Value).IsZeroCents() && (TotalDeposited.Value - TotalRevenueAndOverpayment.Value).IsZeroCents();
            private set { }
        }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.Currency)]
        public decimal DepositControlAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal? TotalDeposited
        {
            get => _totalDeposited = Transactions?.Sum(t => t.Deposited)?.RoundAwayFromZero() ?? _totalDeposited;
            private set => _totalDeposited = value;
        }

        [DataType(DataType.Currency)]
        [Display(Name = "Rev+Ovr")]
        public decimal? TotalRevenueAndOverpayment
        {
            get => _totalRevenueAndOverpayment = Transactions?.Sum(t => t.RevenueAndOverpayment)?.RoundAwayFromZero() ?? _totalRevenueAndOverpayment;
            private set => _totalRevenueAndOverpayment = value;
        }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public virtual List<Transaction> Transactions { get; set; }

        public DateTimeOffset? Posted { get; private set; }

        public DateTimeOffset Created { get; private set; }
        public int CreatedById { get; private set; }

        public virtual Usr.User CreatedBy { get; private set; }
        public virtual IReadOnlyCollection<BatchComment> BatchComments { get; private set; }

        public void Post(bool skipDepositEqualityCheck = false)
        {
            if (IsPosted)
            {
                throw new InvalidOperationException($"The {Name} batch is already posted.");
            }

            if (!skipDepositEqualityCheck && !IsBalanced)
            {
                throw new InvalidOperationException($"The {Name} batch cannot be posted. The batch is not balanced.");
            }

            IsPosted = true;
            Posted = DateTimeOffset.Now;
        }

        public void Unpost()
        {
            if (IsPosted)
            {
                IsPosted = false;
                Posted = null;
            }
        }

        /// <summary>
        /// Returns true if this batch hasn't been posted and balances.
        /// </summary>
        /// <returns></returns>
        public bool CanPost()
        {
            return !IsPosted && IsBalanced;
        }

        /// <summary>
        /// Returns true if this batch hasn't been posted.
        /// </summary>
        /// <returns></returns>
        public bool CanModify()
        {
            return !IsPosted;
        }
    }
}
