using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Fin.Returns
{
    public class TransferReturn
    {
        public int ReturnId { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string DestinationOrganizationId { get; set; } = null!;

        [Required]
        [DataType("PeriodId")]
        public string DestinationPeriodId { get; set; } = null!;

        [Required]
        [DataType("CategoryId")]
        public string DestinationCategoryId { get; set; } = null!;
    }
}
