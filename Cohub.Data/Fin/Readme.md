# Finance Readme

`Cohub.Data.Fin`

## Tax Returns

A tax return is filed by an Organization to a Period and Category.

A key piece of information that is provided by a return is the Taxable Amount. The Taxable Amount is used to calculate the Tax Due.

The Tax Due is combined with the Period's Due Date, the Category and the Payment's Effective Date to calculate line items like penalty and interest, or vendor fee. With this information at hand, Total Due and Payable is calculated.

Estimated Tax Due will be used when the Taxable Amount is not known for a Period and Category that is due. 

The following pseudo-code represents a common way to calculate Estimated Tax Due.

```csharp
// Try to find the most recent amount of tax due that the 
// organization reported for the category (i.e. Sales Tax or
// Lodging Tax) and schedule (i.e. Monthly).
var estimateTaxDue = EstimateBasedOnPreviouslyReportedReturns(organization, category, schedule)

// If the organization has not reported tax due information (or at
// least enough to calculate an estimate) then use the standard
// estimated tax due given a category and schedule.
if (estimateTaxDue is null) taxDueEstimate = StandardEstimatedTaxDue(category, schedule)
```

Note that the `EstimateBasedOnPreviouslyReportedReturns` method could return a value based on one period, or the average of multiple periods, or some other calculation. The `StandardEstimatedTaxDue` method pulls values from configuration.

## Payments

Payment Cases:

* Perfect payment: Payment exactly matches what is currently due and payable by the Payment's Effective Date.
* Overpayment: Payment is greater than what is required by due periods. Payment can be applied to all due periods and what remains is applied as a Credit Operation to the None period and Uncategorized category.
* Underpayment: Payment is less than what is required by due periods. Payment will be applied to the oldest due period first.

Precedence:

* An organization can have multiple filing schedules over a variety of Categories and Periods. In cases like this the oldest periods due take precedence, and then alphabetical order of Category IDs will determine the next level of precedence.

Estimated Tax Due:

* If the Taxable Amount is not known, the taxpayer has not filed a return, then Estimated Tax Due will be calculated.

Other Considerations:

* Active Filers: An organization must have at least one filing schedule in order for due returns to be generated for them.
* Occasional or Unlicensed Filers: Payment can be accepted from any organization in the system. A filing schedule is not required.

## Statements and Assessments

Draft statements are generated for returns that are due. Statements are not generated for payable or closed returns. A draft statement can be deleted, published, or converted to an assessment and published. Once a statement or assessment is published it cannot be modified, only archived.

Assessments vary from statements in a few important ways. Assessments have a due date. Statements do not have a due date because they are informational. An organization should generally only have one published assessment. Publishing another assessment to the same organization pushes back the due date. Once an assessment is addressed by the organization or the city's relationship with the organization is terminated then the assessment should be archived.
