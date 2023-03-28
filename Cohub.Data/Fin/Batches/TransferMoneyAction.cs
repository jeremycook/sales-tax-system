using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin.Batches
{
    public enum TransferMoneyAction
    {
        [Display(Name = "Adjust Dues: Transfer Due to Adj")]
        AdjustDues,

        [Display(Name = "Apply Overpayments: Pay off Due by collecting Ovr into Rev")]
        ApplyOverpayments,

        [Display(Name = "Clear Overpayments: Transfer Ovr to Rev")]
        ClearOverpayments,

        [Display(Name = "Erase Dues: Reduce Due to 0")]
        EraseDues,
    }
}