using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Fin.Views.Transactions
{
    public class ReverseTransaction
    {
        [Required]
        public int Id { get; set; }

        [DataType("BatchId")]
        public int? BatchId { get; set; }

        [DataType(DataType.Currency)]
        public decimal? NSFFee { get; set; }

        public bool OpenRelatedReturns { get; set; }
    }
}
