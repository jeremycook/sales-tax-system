using Cohub.Data.Fin.Returns;
using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using SiteKit.Extensions;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Deposits
{
    public class DepositService
    {
        private readonly CohubDbContext db;
        private readonly IInterestCalculator interestCalculator;

        public DepositService(CohubDbContext db, IInterestCalculator interestCalculator)
        {
            this.db = db;
            this.interestCalculator = interestCalculator;
        }

        /// <summary>
        /// May saves changes to the database multiple times.
        /// Wrap calling this in a transaction to avoid committing changes if an exception is thrown.
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        public async Task<Batch> ProcessDepositInfoAsync(DepositInfo input)
        {
            Batch batch;
            if (input.BatchId == 0)
            {
                batch = new Batch
                {
                    DepositControlAmount = input.Deposits.Sum(o => o.DepositAmount ?? 0),
                    Name = await db.Batches().NextNameAsync("DEP"),
                    Note = $"Deposit",
                    Transactions = new(),
                };
                db.Add(batch);
            }
            else
            {
                batch = await db.Batches(input.BatchId).SingleAsync();
            }

            if (batch.IsPosted)
            {
                throw new DepositServiceException("Cannot modify a posted batch.");
            }

            var invalidDepositAmounts = input.Deposits
                .Select(o => new { o.DepositAmount, PaymentAmount = o.Payments.Sum(p => p.PaymentAmount) })
                .Where(o => o.DepositAmount != o.PaymentAmount);
            if (invalidDepositAmounts.Any())
            {
                throw new DepositServiceException(string.Join(" ", invalidDepositAmounts.Select(o => $"The {o.DepositAmount} deposit amount does not equal {o.PaymentAmount} of payment amounts.")));
            }

            foreach (var deposit in input.Deposits)
            {
                if (deposit.DepositDate == null) throw new ArgumentException($"The {nameof(deposit.DepositDate)} property is null.", nameof(deposit));
                if (deposit.DepositAmount == null) throw new ArgumentException($"The {nameof(deposit.DepositAmount)} property is null.", nameof(deposit));

                var transaction = new Transaction
                {
                    Note = null,
                    Details = new(),
                };
                batch.Transactions.Add(transaction);

                var categoryIds = deposit.Payments.Select(o => o.CategoryId).Distinct().ToArray();
                var periodIds = deposit.Payments.Select(o => o.PeriodId).Distinct().ToArray();

                var netDeposit = TransactionDetail.NetDeposit(
                    organizationId: deposit.DepositorId,
                    categoryId: categoryIds.Length == 1 ? categoryIds[0] : CategoryId.Uncategorized,
                    periodId: periodIds.Length == 1 ? periodIds[0] : PeriodId.None,
                    effectiveDate: deposit.DepositDate.Value,
                    amount: deposit.DepositAmount.Value);
                transaction.Details.Add(netDeposit);

                foreach (var payment in deposit.Payments)
                {
                    if (payment.PaymentAmount == null) throw new ArgumentException($"The {nameof(payment.PaymentAmount)} property is null.", nameof(payment));

                    // TODO: Can this be done without immediately saving changes?
                    var @return = await AddOrUpdateReturnAsync(deposit, payment);
                    await db.SaveChangesAsync();

                    if (!input.AllowMissingFilings && @return.GetLatestFiling() == null)
                    {
                        throw new MissingFilingsDepositServiceException($"The {@return} return does not have any filings or assessments.");
                    }

                    var returnBalance =
                        await db.ReturnBalances(o => o.Id == @return.Id).SingleOrDefaultAsync() ??
                        throw new DepositServiceException($"Unable to determine return balance for {payment.OrganizationId} organization, {payment.CategoryId} category and {payment.PeriodId} period.");

                    var unusedPayment = PayReturn(transaction, returnBalance, deposit.DepositDate.Value, payment.PaymentAmount.Value);

                    if (unusedPayment > 0)
                    {
                        var payable = TransactionDetail.NetOverpayment(@return.CategoryId, @return.OrganizationId, @return.PeriodId, deposit.DepositDate.Value, unusedPayment);
                        transaction.Details.Add(payable);
                    }
                }
            }

            await db.SaveChangesAsync();

            return batch;
        }

        /// <summary>
        /// Adds <see cref="TransactionDetail"/>s to the <paramref name="transaction"/>
        /// and returns unused payment.
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="returnBalance"></param>
        /// <param name="paymentDate"></param>
        /// <param name="paymentAmount"></param>
        /// <returns></returns>
        /// <exception cref="DepositServiceException">If the unused payment is less than 0.</exception>
        private decimal PayReturn(Transaction transaction, ReturnBalance returnBalance, DateTime paymentDate, decimal paymentAmount)
        {
            var snapshot = CalculatePaymentSnapshot(paymentDate, returnBalance);
            var @return = returnBalance.Return;

            var unusedPayment = paymentAmount;

            // 1. Tax
            {
                var paidOrAdjusted = Math.Min(snapshot.NetAmount, returnBalance.TotalNetRevAndAdj);
                var due = snapshot.NetAmount - paidOrAdjusted;

                var payment = Math.Max(0, Math.Min(due, unusedPayment).RoundAwayFromZero());
                if (payment >= 0)
                {
                    TransactionDetail td = TransactionDetail.NetRevenue(@return.CategoryId, @return.OrganizationId, @return.PeriodId, paymentDate, payment);
                    if (snapshot.VendorFee > 0)
                    {
                        td.Note = $"{snapshot.VendorFee:C2} vendor fee";
                    }
                    transaction.Details.Add(td);
                }

                var dueAfterPayment = due - payment;

                var offsetDue = (dueAfterPayment - returnBalance.NetDue).RoundAwayFromZero();
                if (offsetDue != 0)
                {
                    transaction.Details.Add(TransactionDetail.NetDue(@return.CategoryId, @return.OrganizationId, @return.PeriodId, paymentDate, offsetDue));
                }

                unusedPayment -= payment;
            }

            // 2. Interest
            {
                var paidOrAdjusted = Math.Min(snapshot.InterestDue, returnBalance.TotalInterestRevAndAdj);
                var due = snapshot.InterestDue - paidOrAdjusted;

                var payment = Math.Max(0, Math.Min(due, unusedPayment).RoundAwayFromZero());
                if (payment > 0)
                {
                    transaction.Details.Add(TransactionDetail.InterestRevenue(@return.CategoryId, @return.OrganizationId, @return.PeriodId, paymentDate, payment));
                }

                var dueAfterPayment = due - payment;

                var offsetDue = (dueAfterPayment - returnBalance.InterestDue).RoundAwayFromZero();
                if (offsetDue != 0)
                {
                    transaction.Details.Add(TransactionDetail.InterestDue(@return.CategoryId, @return.OrganizationId, @return.PeriodId, paymentDate, offsetDue));
                }

                unusedPayment -= payment;
            }

            // 3. Penalty
            {
                var paidOrAdjusted = Math.Min(snapshot.PenaltyDue, returnBalance.TotalPenaltyRevAndAdj);
                var due = snapshot.PenaltyDue - paidOrAdjusted;

                var payment = Math.Max(0, Math.Min(due, unusedPayment).RoundAwayFromZero());
                if (payment > 0)
                {
                    transaction.Details.Add(TransactionDetail.PenaltyRevenue(@return.CategoryId, @return.OrganizationId, @return.PeriodId, paymentDate, payment));
                }

                var dueAfterPayment = due - payment;

                var offsetDue = (dueAfterPayment - returnBalance.PenaltyDue).RoundAwayFromZero();
                if (offsetDue != 0)
                {
                    transaction.Details.Add(TransactionDetail.PenaltyDue(@return.CategoryId, @return.OrganizationId, @return.PeriodId, paymentDate, offsetDue));
                }

                unusedPayment -= payment;
            }

            unusedPayment = unusedPayment.RoundAwayFromZero();

            if (unusedPayment < 0)
            {
                throw new DepositServiceException($"The unused payment of {unusedPayment} from paying the {@return} return is less than 0.");
            }

            return unusedPayment;
        }

        /// <summary>
        /// Returns the matching return without saving any changes to the database.
        /// If a match is found in the database it will be updated and returned. 
        /// If a match is not found a new <see cref="Return"/> will be activated and added to the <see cref="db"/>.
        /// </summary>
        /// <param name="deposit"></param>
        /// <param name="payment"></param>
        /// <returns></returns>
        public async Task<Return> AddOrUpdateReturnAsync(Deposit deposit, DepositPayment payment)
        {
            if (deposit.DepositDate == null) throw new ArgumentException($"The {nameof(deposit.DepositDate)} property is null.", nameof(deposit));
            if (payment.OrganizationId == null) throw new ArgumentException($"The {nameof(payment.OrganizationId)} property is null.", nameof(payment));
            if (payment.CategoryId == null) throw new ArgumentException($"The {nameof(payment.CategoryId)} property is null.", nameof(payment));
            if (payment.PeriodId == null) throw new ArgumentException($"The {nameof(payment.PeriodId)} property is null.", nameof(payment));

            var @return = await db.Returns()
                .IncludeReferences()
                .IncludeCollections()
                .SingleOrDefaultAsync(o =>
                    o.OrganizationId == payment.OrganizationId &&
                    o.PeriodId == payment.PeriodId &&
                    o.CategoryId == payment.CategoryId
                );

            var organization =
                await db.Organizations().FindAsync(payment.OrganizationId) ??
                throw new ArgumentException($"The '{payment.OrganizationId}' OrganizationId was not found.", nameof(payment));
            await db.Entry(organization).Collection(o => o.Labels).LoadAsync();

            var category = await db.Categories().FindAsync(payment.CategoryId) ??
                throw new ArgumentException($"The '{payment.CategoryId}' CategoryId was not found.", nameof(payment));

            var period = await db.Periods().FindAsync(payment.PeriodId) ??
                throw new ArgumentException($"The '{payment.PeriodId}' PeriodId was not found.", nameof(payment));

            if (@return == null)
            {
                @return = new Return
                {
                    StatusId = period.IsDue() ? ReturnStatusId.Due : ReturnStatusId.Payable,
                    OrganizationId = organization.Id,
                    CategoryId = category.Id,
                    PeriodId = period.Id,
                    Labels = organization.Labels.ToList(),
                    Filings = new(),
                };
                db.Add(@return);
            }

            var missingLabels = organization.Labels.Except(@return.Labels);
            @return.Labels.AddRange(missingLabels);

            if (payment.Assessment != null)
            {
                var filing = @return.GetLatestFiling() as AssessmentFiling;
                if (filing == null || filing.AssessmentAmount != payment.Assessment)
                {
                    @return.Filings.Add(new AssessmentFiling(DateTimeOffset.Now)
                    {
                        AssessmentAmount = payment.Assessment.Value,
                        FilingDate = deposit.DepositDate.Value,
                    });
                    @return.HasFiled = true;
                }
            }

            if (payment.Fees != null)
            {
                if (category.TypeId != CategoryTypeId.Fee)
                {
                    throw new DepositServiceException($"Fees cannot be entered for the {category} category.");
                }

                var filing = @return.GetLatestFiling() as FeeFiling;
                if (filing == null || filing.FeeAmount != payment.Fees)
                {
                    @return.Filings.Add(new FeeFiling(DateTimeOffset.Now)
                    {
                        FeeAmount = payment.Fees.Value,
                        FilingDate = deposit.DepositDate.Value,
                    });
                    @return.HasFiled = true;
                }
            }

            if (payment.Taxable != null || payment.Excess != null)
            {
                if (category.TypeId != CategoryTypeId.Tax)
                {
                    throw new DepositServiceException($"Taxable amount cannot be entered for the {category} category.");
                }

                var filing = @return.GetLatestFiling() as TaxFiling;
                if (filing == null || filing.TaxableAmount != payment.Taxable || filing.ExcessTax != payment.Excess)
                {
                    if (filing == null || filing.FilingDate != deposit.DepositDate)
                    {
                        filing = new TaxFiling(DateTimeOffset.Now)
                        {
                            FilingDate = deposit.DepositDate.Value,
                        };
                        @return.Filings.Add(filing);
                    }

                    filing.TaxableAmount = payment.Taxable.GetValueOrDefault();
                    filing.ExcessTax = payment.Excess.GetValueOrDefault();
                    @return.HasFiled = true;
                }
            }

            return @return;
        }

        public PaymentSnapshot CalculatePaymentSnapshot(DateTime paymentDate, ReturnBalance returnBalance)
        {
            if (returnBalance is null)
            {
                throw new ArgumentNullException(nameof(returnBalance));
            }

            var period = returnBalance.Period;
            var paymentConfiguration = returnBalance.PaymentConfiguration;

            var paidOnTime = paymentDate <= period.DueDate;

            decimal netAmount;
            decimal vendorFee = 0;
            decimal penaltyDue = 0;
            decimal interestDue = 0;

            if (returnBalance.Category.TypeId == CategoryTypeId.Tax &&
                returnBalance.LastFiling is TaxFiling taxFiling)
            {
                netAmount = 0.01m * paymentConfiguration.TaxPercentage * taxFiling.TaxableAmount;

                netAmount += taxFiling.ExcessTax;

                if (paidOnTime)
                {
                    // Calculate vendor fee if paid on time and taxable amount was provided

                    // Vendor fee: Deduct x.xx% from net tax due with a max of $xx.xx
                    vendorFee = 0.01m * paymentConfiguration.VendorFeePercentage * netAmount;

                    if (vendorFee > paymentConfiguration.VendorFeeMax)
                    {
                        vendorFee = paymentConfiguration.VendorFeeMax;
                    }

                    // The vendor fee reduces net tax due
                    netAmount -= vendorFee;
                }
            }
            else if (returnBalance.Category.TypeId == CategoryTypeId.Fee &&
                returnBalance.LastFiling is FeeFiling feeFiling)
            {
                netAmount = feeFiling.FeeAmount;
            }
            else if (returnBalance.LastFiling is AssessmentFiling assessmentFiling)
            {
                netAmount = assessmentFiling.AssessmentAmount;
            }
            else if (returnBalance.Period.IsDue())
            {
                netAmount = paymentConfiguration.EstimateNetAmountDue(returnBalance.HistoricNetAmountDue);
            }
            else
            {
                netAmount = 0;
            }

            var netDue = netAmount - returnBalance.TotalNetRevAndAdj;

            if (!paidOnTime)
            {
                // Late payment

                // Penalty and interest is calculated based on the net tax due
                // that was not paid on time. If they paid on time for some of it
                // then that does not count against them.
                var lateNetDue = netAmount - returnBalance.OnTimeNetRevAndAdj;

                // Penalty: Add x.xx% of net tax due to total due and payable
                if (paymentConfiguration.PenaltyPercentage > 0)
                {
                    penaltyDue = .01m * paymentConfiguration.PenaltyPercentage * lateNetDue;
                }

                // Interest: Add x.xx% of net tax due per month to total due and payable
                if (paymentConfiguration.InterestPercentage > 0)
                {
                    interestDue = interestCalculator.CalculateInterest(paymentConfiguration.InterestPercentage, period.DueDate, paymentDate, lateNetDue);
                }
            }

            var snapshot = new PaymentSnapshot(
                paymentDate: paymentDate,
                dueDate: period.DueDate,
                hasFiling: returnBalance.LastFiling != null,
                vendorFee: vendorFee,
                netAmount: netAmount,
                netDue: netDue,
                penaltyDue: penaltyDue,
                interestDue: interestDue);
            return snapshot;
        }
    }
}
