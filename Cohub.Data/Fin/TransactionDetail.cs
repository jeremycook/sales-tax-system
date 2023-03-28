using SiteKit.Extensions;
using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class TransactionDetail
    {
        private decimal _amount;

        public TransactionDetail()
        {
        }

        public TransactionDetail(DateTimeOffset created)
        {
            Created = created;
        }

        public TransactionDetail(TransactionDetail input)
        {
            TransactionId = input.TransactionId;
            UpdateWith(input);
        }

        public void UpdateWith(TransactionDetail input)
        {
            BucketId = input.BucketId;
            CategoryId = input.CategoryId;
            SubcategoryId = input.SubcategoryId;
            OrganizationId = input.OrganizationId;
            PeriodId = input.PeriodId;
            EffectiveDate = input.EffectiveDate;
            Amount = input.Amount;
            Note = input.Note;
        }

        public static TransactionDetail NetDeposit(string organizationId, string categoryId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Deposit,
                OrganizationId = organizationId,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Net,
                PeriodId = periodId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail NSFFeeDue(string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Due,
                CategoryId = Fin.CategoryId.NSFFee,
                SubcategoryId = Fin.SubcategoryId.Net,
                OrganizationId = organizationId,
                PeriodId = periodId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail NetOverpayment(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Overpayment,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Net,
                PeriodId = periodId,
                OrganizationId = organizationId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail NetDue(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Due,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Net,
                PeriodId = periodId,
                OrganizationId = organizationId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail PenaltyDue(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Due,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Penalty,
                PeriodId = periodId,
                OrganizationId = organizationId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail InterestDue(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Due,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Interest,
                PeriodId = periodId,
                OrganizationId = organizationId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail NetRevenue(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Revenue,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Net,
                PeriodId = periodId,
                OrganizationId = organizationId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail PenaltyRevenue(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Revenue,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Penalty,
                PeriodId = periodId,
                OrganizationId = organizationId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public static TransactionDetail InterestRevenue(string categoryId, string organizationId, string periodId, DateTime effectiveDate, decimal amount)
        {
            return new TransactionDetail
            {
                BucketId = Fin.BucketId.Revenue,
                CategoryId = categoryId,
                SubcategoryId = Fin.SubcategoryId.Interest,
                OrganizationId = organizationId,
                PeriodId = periodId,
                EffectiveDate = effectiveDate,
                Amount = amount,
            };
        }

        public override string ToString()
        {
            return $"#{Id}";
        }

        public int Id { get; set; }

        public int TransactionId { get; set; }

        [Required]
        [DataType("BucketId")]
        public string BucketId { get; set; }

        [Required]
        [DataType("CategoryId")]
        public string CategoryId { get; set; }

        [Required]
        [DataType("SubcategoryId")]
        public string SubcategoryId { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string OrganizationId { get; set; }

        [Required]
        [DataType("PeriodId")]
        public string PeriodId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime EffectiveDate { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get => _amount; set => _amount = value.RoundAwayFromZero(); }

        [DataType(DataType.MultilineText)]
        public string Note { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual Bucket Bucket { get; private set; }
        public virtual Category Category { get; private set; }
        public virtual Subcategory Subcategory { get; private set; }
        public virtual Org.Organization Organization { get; private set; }
        public virtual Period Period { get; private set; }
        public virtual Transaction Transaction { get; private set; }
    }
}
