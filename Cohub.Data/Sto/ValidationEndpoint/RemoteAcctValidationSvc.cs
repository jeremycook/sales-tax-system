using Cohub.Data.Fin;
using Cohub.Data.Org;
using Cohub.Data.Sto.Configuration;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Linq;
using System.ServiceModel;
using System.Threading.Tasks;

#nullable disable
#pragma warning disable IDE0060 // Remove unused parameter

namespace Cohub.Data.Sto.ValidationEndpoint
{
    [ServiceContract(Namespace = "http://www.salestaxonline.com/RemoteAcctValidationSvc/")]
    public class RemoteAcctValidationSvc
    {
        private readonly StoOptions stoOptions;
        private readonly CohubDbContext db;
        private readonly ILogger<RemoteAcctValidationSvc> logger;

        public RemoteAcctValidationSvc(CohubDbContext db, IOptions<StoOptions> stoOptions, ILogger<RemoteAcctValidationSvc> logger)
        {
            this.stoOptions = stoOptions.Value;
            this.db = db;
            this.logger = logger;
        }

        [OperationContract(Action = "http://www.salestaxonline.com/RemoteAcctValidationSvc/ValidateTaxpayerAccount")]
        public async Task<ValidateTaxpayerAccountResult> ValidateTaxpayerAccount(
            string AccessKey,
            string TaxAccountID,
            string ReturnTypeCode,
            string FilingFrequCode,
            string StateTaxID,
            string ValidationCompanyName,
            string ValidationAddress,
            string ValidationZip,
            string ValidationPhone
        )
        {
            try
            {
                if (!stoOptions.ReturnCodeToCategoryMap.TryGetValue(ReturnTypeCode, out string categoryId))
                {
                    categoryId = ReturnTypeCode;
                }
                if (!await db.Categories().AnyAsync(o => o.Id == categoryId))
                {
                    logger.LogError($"Invalid category ID: {categoryId}");
                }

                if (!stoOptions.FilingStatusToFrequencyMap.TryGetValue(FilingFrequCode, out string frequencyId))
                {
                    frequencyId = FilingFrequCode;
                }
                if (!await db.Frequencies().AnyAsync(o => o.Id == frequencyId))
                {
                    logger.LogError($"Invalid frequency ID: {frequencyId}");
                }

                var organization =
                    await db.Organizations()
                        .Include(o => o.FilingSchedules).ThenInclude(o => o.PaymentChart)
                        .SingleOrDefaultAsync(o => o.Id.ToUpper() == TaxAccountID.ToUpper()) ??
                    await db.Organizations()
                        .Include(o => o.FilingSchedules).ThenInclude(o => o.PaymentChart)
                        .SingleOrDefaultAsync(o => o.StateID.ToUpper() == StateTaxID.ToUpper());

                if (organization is null)
                {
                    return new ValidateTaxpayerAccountResult
                    {
                        Code = "FAIL",
                        Detail = "Error: The account number or state tax ID is invalid.",
                        AdditionalInformation = string.Empty,
                    };
                }
                else if (!organization.OnlineFiler)
                {
                    if (organization.StatusId == OrganizationStatusId.Expired)
                    {
                        return new ValidateTaxpayerAccountResult
                        {
                            Code = "FAIL",
                            Detail = $"Error: Your account is expired and filing online is disabled for this account. Please contact the City of AnywhereUSA to initiate the renewal process.",
                            AdditionalInformation = string.Empty,
                        };
                    }
                    else
                    {
                        return new ValidateTaxpayerAccountResult
                        {
                            Code = "FAIL",
                            Detail = $"Error: Filing online is disabled for this account.",
                            AdditionalInformation = string.Empty,
                        };
                    }
                }

                if (!organization.FilingSchedules.Any(o =>
                    o.PaymentChart.CategoryId == categoryId &&
                    o.PaymentChart.FrequencyId == frequencyId
                ))
                {
                    return new ValidateTaxpayerAccountResult
                    {
                        Code = "FAIL",
                        Detail = $"Error: The {frequencyId} filing frequency, and {categoryId} category is invalid for this account.",
                        AdditionalInformation = string.Empty,
                    };
                }

                return new ValidateTaxpayerAccountResult
                {
                    Code = "OK",
                    Detail = string.Empty,
                    AdditionalInformation = string.Empty,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Suppressed {ex.GetType()}: {ex.OriginalMessage()}");
                return new ValidateTaxpayerAccountResult
                {
                    Code = "FAIL",
                    Detail = $"Error: {ex.GetType().Name.Humanize()}",
                    AdditionalInformation = string.Empty,
                };
            }
        }

        [OperationContract(Action = "http://www.salestaxonline.com/RemoteAcctValidationSvc/ValidateTaxpayerReturn")]
        public async Task<ValidateTaxpayerReturnResult> ValidateTaxpayerReturn(
            string AccessKey,
            string TaxAccountID,
            string ReturnTypeCode,
            string FilingFrequCode,
            string FilingPeriod
        )
        {
            try
            {
                if (!stoOptions.ReturnCodeToCategoryMap.TryGetValue(ReturnTypeCode, out string categoryId))
                {
                    categoryId = ReturnTypeCode;
                }
                if (!await db.Categories().AnyAsync(o => o.Id == categoryId))
                {
                    logger.LogError($"Invalid category ID: {categoryId}");
                }

                if (!stoOptions.FilingStatusToFrequencyMap.TryGetValue(FilingFrequCode, out string frequencyId))
                {
                    frequencyId = FilingFrequCode;
                }
                if (!await db.Frequencies().AnyAsync(o => o.Id == frequencyId))
                {
                    logger.LogError($"Invalid frequency ID: {frequencyId}");
                }

                var organization =
                    await db.Organizations()
                        .Include(o => o.FilingSchedules).ThenInclude(o => o.PaymentChart)
                        .SingleOrDefaultAsync(o => o.Id.ToUpper() == TaxAccountID.ToUpper());

                if (organization is null)
                {
                    return new ValidateTaxpayerReturnResult
                    {
                        Code = "FAIL",
                        Detail = "Error: The account number or state tax ID is invalid.",
                        AdditionalInformation = string.Empty,
                    };
                }
                else if (!organization.OnlineFiler)
                {
                    return new ValidateTaxpayerReturnResult
                    {
                        Code = "FAIL",
                        Detail = $"Error: Filing online is not enabled for this account.",
                        AdditionalInformation = string.Empty,
                    };
                }

                if (!organization.FilingSchedules.Any(o =>
                    o.PaymentChart.CategoryId == categoryId &&
                    o.PaymentChart.FrequencyId == frequencyId
                ))
                {
                    return new ValidateTaxpayerReturnResult
                    {
                        Code = "FAIL",
                        Detail = $"Error: The {frequencyId} filing frequency, and {categoryId} category is invalid for this account.",
                        AdditionalInformation = string.Empty,
                    };
                }

                var periodEndDate = DateTime.Parse(FilingPeriod);
                var @return = await db.Returns()
                    .Include(o => o.Period)
                    .FirstOrDefaultAsync(o =>
                        o.OrganizationId == organization.Id &&
                        o.CategoryId == categoryId &&
                        (o.Period.StartDate <= periodEndDate && periodEndDate <= o.Period.EndDate)
                    );

                if (@return is null)
                {
                    return new()
                    {
                        Code = "FAIL",
                        Detail = $"Error: The {frequencyId} filing frequency, {categoryId} category, and {periodEndDate:d} filing date is invalid for this account.",
                        AdditionalInformation = string.Empty,
                    };
                }
                else if (@return.StatusId != ReturnStatusId.Payable && @return.StatusId != ReturnStatusId.Due)
                {
                    return new()
                    {
                        Code = "FAIL",
                        Detail = $"Error: The {frequencyId} filing frequency, {categoryId} category, and {periodEndDate:d} filing date is invalid for this account because the return is not payable or due.",
                        AdditionalInformation = string.Empty,
                    };
                }
                else if (DateTime.Today <= @return.Period.EndDate)
                {
                    return new()
                    {
                        Code = "FAIL",
                        Detail = $"Error: The {frequencyId} {@return.Period} {categoryId} return cannot be filed online until {@return.Period.EndDate.AddDays(1):d}, after the period has ended. If you are filing early because your business is closing you can still file by mail or in-person, or by e-mail if it is a zero return.",
                        AdditionalInformation = string.Empty,
                    };
                }

                return new()
                {
                    Code = "OK",
                    Detail = string.Empty,
                    AdditionalInformation = string.Empty,
                };
            }
            catch (Exception ex)
            {
                logger.LogError(ex, $"Suppressed {ex.GetType()}: {ex.OriginalMessage()}");
                return new()
                {
                    Code = "FAIL",
                    Detail = $"Error: {ex.GetType().Name.Humanize()}",
                    AdditionalInformation = string.Empty,
                };
            }
        }
    }
}
