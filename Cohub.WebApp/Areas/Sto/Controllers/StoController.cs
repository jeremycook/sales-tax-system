using Cohub.Data;
using Cohub.Data.Fin;
using Cohub.Data.Fin.Deposits;
using Cohub.Data.Org;
using Cohub.Data.Sto.Configuration;
using Cohub.Data.Sto.Services;
using Cohub.Data.Sto.Xml;
using Cohub.Data.Usr;
using Cohub.WebApp.Areas.Sto.Views.Sto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SiteKit.Info;
using SiteKit.Text;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Cohub.WebApp.Areas.Sto.Controllers
{
    [Authorize(Policy.Process)]
    [Area("Sto")]
    [Route("sto/sto-files")]
    public class StoController : Controller
    {
        private readonly ILogger<StoController> logger;
        private readonly Actor actor;
        private readonly IOptions<StoOptions> stoOptions;
        private readonly IStringLocalizer<StoController> l;
        private readonly CohubDbContext db;
        private readonly DepositService depositService;
        private readonly IServiceProvider serviceProvider;

        public StoController(
            ILogger<StoController> logger,
            Actor actor,
            IOptions<StoOptions> stoOptions,
            IStringLocalizer<StoController> l,
            CohubDbContext db,
            DepositService depositService,
            IServiceProvider serviceProvider)
        {
            this.logger = logger;
            this.actor = actor;
            this.stoOptions = stoOptions;
            this.l = l;
            this.db = db;
            this.depositService = depositService;
            this.serviceProvider = serviceProvider;
        }


        [HttpGet("import")]
        public IActionResult Import()
        {
            var input = new ImportInput();

            ViewBag._Title = l["STO Import"];
            return View(input);
        }

        [HttpPost("import")]
        public async Task<IActionResult> Import(ImportInput input)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(Returns));
                    using Stream stream = input.STOXMLFile!.OpenReadStream();
                    using StreamReader reader = new StreamReader(stream);
                    Returns stoReturns = (Returns)serializer.Deserialize(reader)!;

                    using var tx = await db.Database.BeginTransactionAsync();

                    // Start batch way up here because it will be referenced by comments
                    var batch = new Batch()
                    {
                        Name = await db.Batches().NextNameAsync("STO"),
                        Note = $"Generated from the {Path.GetFileName(input.STOXMLFile.FileName)} STO file.",
                        DepositControlAmount = stoReturns.Return.Sum(r => r.Return_info.Amount_due),
                        Transactions = new List<Transaction>(),
                    };
                    db.Add(batch);

                    var taxpayerIDs = stoReturns.Return.Select(o => o.Return_info.TaxpayerID_number).ToArray();
                    var stateIDs = stoReturns.Return
                        .Select(o => o.Taxpayer_info.Statetaxid)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToArray();
                    var federalIDs = stoReturns.Return
                        .Select(o => o.Taxpayer_info.FedEmplID)
                        .Where(id => !string.IsNullOrWhiteSpace(id))
                        .ToArray();

                    var candidateOrganizations = await db.Organizations()
                        .Where(o =>
                            taxpayerIDs.Contains(o.Id) ||
                            federalIDs.Contains(o.FederalID) ||
                            stateIDs.Contains(o.StateID)
                        )
                        .ToListAsync();

                    var returns = stoReturns.Return.Select(r => new ReturnData
                    {
                        StoReturn = r,
                        Organization = candidateOrganizations.FirstOrDefault(o => o.Id == r.Return_info.TaxpayerID_number)!,
                        CategoryId = stoOptions.Value.ReturnCodeToCategoryMap.TryGetValue(r.Return_info.Return_Code, out var catId) ? catId :
                            throw new NotSupportedException($"A corresponding category could not be determined for the {r} STO return."),
                    }).ToArray();

                    // If unable to match organization to the provided taxpayer ID then try alternative methods.
                    var mapper = new StoReturnToOrganizationMapper();
                    foreach (var data in returns.Where(o => o.Organization is null).ToArray())
                    {
                        var matches = candidateOrganizations
                            .Where(o =>
                                o.Id == data.StoReturn.Return_info.TaxpayerID_number ||
                                (o.StateID != null && o.StateID == data.StoReturn.Taxpayer_info.Statetaxid) ||
                                (o.FederalID != null && o.FederalID == data.StoReturn.Taxpayer_info.FedEmplID)
                            )
                            .ToArray();
                        if (matches.Length == 1)
                        {
                            data.Organization = matches[0];
                        }
                        else if (matches.Length > 1)
                        {
                            // Matching on Id is considered an exact match
                            if (matches.Where(o => o.Id == data.StoReturn.Return_info.ID) is var byIds && byIds.Count() == 1)
                            {
                                data.Organization = byIds.First();
                            }
                            // Try narrowing results by matching both IDs
                            else if (
                                !string.IsNullOrWhiteSpace(data.StoReturn.Taxpayer_info.Statetaxid) &&
                                !string.IsNullOrWhiteSpace(data.StoReturn.Taxpayer_info.FedEmplID) &&
                                matches.Where(o =>
                                    (o.StateID != null && o.StateID == data.StoReturn.Taxpayer_info.Statetaxid) &&
                                    (o.FederalID != null && o.FederalID == data.StoReturn.Taxpayer_info.FedEmplID)
                                ) is var byBothIDs && byBothIDs.Count() == 1)
                            {
                                data.Organization = byBothIDs.First();
                            }
                            // Try narrowing results by matching state ID
                            else if (
                                !string.IsNullOrWhiteSpace(data.StoReturn.Taxpayer_info.Statetaxid) &&
                                matches.Where(o =>
                                    o.StateID != null && o.StateID == data.StoReturn.Taxpayer_info.Statetaxid
                                ) is var byStateID && byStateID.Count() == 1)
                            {
                                data.Organization = byStateID.First();
                            }
                            // Try narrowing results by matching federal ID
                            else if (
                                !string.IsNullOrWhiteSpace(data.StoReturn.Taxpayer_info.FedEmplID) &&
                                matches.Where(o =>
                                    o.FederalID != null && o.FederalID == data.StoReturn.Taxpayer_info.FedEmplID
                                ) is var byFederalID && byFederalID.Count() == 1)
                            {
                                data.Organization = byFederalID.First();
                            }
                            // Try narrowing results by DBA
                            else if (matches.Where(o =>
                                     !string.IsNullOrWhiteSpace(o.Dba) &&
                                     string.Equals(data.StoReturn.Taxpayer_info.Company, o.Dba, StringComparison.InvariantCultureIgnoreCase)
                                 ) is var byDba && byDba.Count() == 1)
                            {
                                data.Organization = byDba.First();
                            }
                            // Try narrowing results by organization name
                            else if (matches.Where(o =>
                                    string.Equals(data.StoReturn.Taxpayer_info.Company, o.OrganizationName, StringComparison.InvariantCultureIgnoreCase)
                                ) is var byName && byName.Count() == 1)
                            {
                                data.Organization = byName.First();
                            }
                            else
                            {
                                throw new NotSupportedException($"Halting: Could not exactly match one organization to the '{data.StoReturn.Taxpayer_info.Company}' STO taxpayer company even though {matches.Length} candidate organizations were matched based on FEIN or State License Number.");
                            }
                        }
                        else
                        {
                            // No matches, create and assign an organization
                            var org = new Organization()
                            {
                                Id = Regex.IsMatch(data.StoReturn.Return_info.TaxpayerID_number, @"\d+[A-Z]?") ?
                                       data.StoReturn.Return_info.TaxpayerID_number :
                                       await db.Organizations().NextIdAsync(),
                                StatusId = data.StoReturn.Return_info.Amount_due > 0 ?
                                       OrganizationStatusId.Pending :
                                       OrganizationStatusId.Unlicensed,
                            };
                            db.Add(org);
                            mapper.Map(org, data.StoReturn);
                            db.Comment($"Created organization based on STO return {data.StoReturn}.", new OrganizationComment(org.Id), new BatchComment(batch));

                            data.Organization = org;

                            // Add to candidates in case one business filed more than one return
                            candidateOrganizations.Add(org);
                        }
                    }

                    // Set periods
                    var stoFilingStatuses = returns
                        .Select(r => r.StoReturn.Return_info.Filing_status)
                        .Distinct()
                        .Select(r => stoOptions.Value.FilingStatusToFrequencyMap.TryGetValue(r, out var frequencyId) ? frequencyId : r)
                        .ToArray();
                    var stoFilePeriodFromDates = returns
                        .Select(r => r.StoReturn.Return_info.File_period_DateTimeOffset().LocalDateTime.Date.AddDays(1))
                        .Distinct()
                        .ToArray();
                    var candidatePeriods = await db.Set<Period>()
                        .Where(o =>
                            stoFilingStatuses.Contains(o.FrequencyId) &&
                            stoFilePeriodFromDates.Contains(o.EndDate)
                        )
                        .ToArrayAsync();
                    foreach (var data in returns)
                    {
                        var filingPeriodEndDate = data.StoReturn.Return_info.File_period_DateTimeOffset().LocalDateTime.Date.AddDays(1);
                        var matches = candidatePeriods
                            .Where(o =>
                                o.FrequencyId == (stoOptions.Value.FilingStatusToFrequencyMap.TryGetValue(data.StoReturn.Return_info.Filing_status, out var frequencyId) ? frequencyId : data.StoReturn.Return_info.Filing_status) &&
                                o.EndDate == filingPeriodEndDate
                            );
                        if (matches.Count() == 1)
                        {
                            data.Period = matches.First();
                        }
                        else if (matches.Count() > 1)
                        {
                            throw new NotSupportedException($"Could not find a period that exclusively matches the {data.StoReturn.Return_info.Filing_status} filing status and {filingPeriodEndDate:d} filing period end date from STO for {data.StoReturn}. {matches.Count()} candidate periods were found.");
                        }
                        else
                        {
                            throw new NotSupportedException($"Could not find a period that matches the {data.StoReturn.Return_info.Filing_status} filing status and {filingPeriodEndDate:d} filing period end date from STO for {data.StoReturn}.");
                        }
                    }

                    var duplicates = returns.GroupBy(o => new
                    {
                        o.Organization,
                        o.CategoryId,
                        PeriodId = o.Period.Id
                    }).Where(g => g.Count() > 1);
                    if (duplicates.Any())
                    {
                        throw new NotSupportedException($"Duplicate returns were found in the file. Please handle the issue in STO and re-download the file. Duplicates: {string.Join(", ", duplicates.Select(g => "(" + g.Key.Organization + "," + g.Key.PeriodId + "," + g.Key.CategoryId + ")"))}");
                    }

                    // Save any changes that affect the database because 
                    // the next step expects the data to be in the database.
                    await db.SaveChangesAsync();

                    // Build deposit
                    var depositInfo = new DepositInfo
                    {
                        BatchId = batch.Id,
                    };
                    foreach (var returnData in returns)
                    {
                        var deposit = new Deposit
                        {
                            DepositorId = returnData.Organization.Id,
                            DepositDate = returnData.StoReturn.Return_info.File_date_DateTimeOffset().LocalDateTime.Date,
                            DepositAmount = returnData.StoReturn.Return_info.Amount_due,
                        };
                        depositInfo.Deposits.Add(deposit);
                        var payment = new DepositPayment
                        {
                            PaymentAmount = returnData.StoReturn.Return_info.Amount_due,
                            OrganizationId = returnData.Organization.Id,
                            CategoryId = returnData.CategoryId,
                            PeriodId = returnData.Period.Id,
                            ReturnId = returnData.Return?.Id,
                            Taxable = returnData.StoReturn.User_entries.User_entry.Single(o => o.Name == "Line5").AsDecimal ?? 0,
                            Excess = returnData.StoReturn.User_entries.User_entry.Single(o => o.Name == "Line7").AsDecimal ?? 0,
                        };
                        deposit.Payments.Add(payment);
                    }

                    // Process deposit
                    batch = await depositService.ProcessDepositInfoAsync(depositInfo);

                    // And done
                    await tx.CommitAsync();

                    TempData.Success(Html.Interpolate($"Imported {stoReturns.Return.Count} STO returns, and opened the <a href=\"{Url.Action("Details", "Batches", new { area = "Fin", id = batch.Id })}\">{batch}</a>. Please review and post the batch."));
                    return RedirectToAction(nameof(Import));
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, $"Suppressed {ex.GetType()}: {ex.Message}");
                    ModelState.AddModelError(string.Empty, l["Exception: {0}", ex.OriginalMessage()] ?? string.Empty);
                }
            }

            ViewBag._Title = l["STO Import"];
            return View(input);
        }
    }
}
