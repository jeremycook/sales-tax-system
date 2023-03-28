using Cohub.Data.Fin;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

#nullable disable

namespace Cohub.Data.AnywhereUSA
{
    [Table("gl_account_allocation", Schema = "anywhereusa")]
    public class GlAccountAllocation
    {
        /// <summary>
        /// A category like "Sales Tax" or "License Fee".
        /// </summary>
        [Required]
        public string CategoryId { get; set; }

        /// <summary>
        /// A subcategory like "Net" or "Penalty".
        /// </summary>
        [Required]
        public string SubcategoryId { get; set; }

        /// <summary>
        /// A GL account number like "10-00-0000-01000".
        /// </summary>
        [Required]
        public string GlAccountNumber { get; set; }

        /// <summary>
        /// A positive or negative percent.
        /// </summary>
        [Range(-100, 100)]
        public decimal Percent { get; set; }

        public virtual Category Category { get; private set; }
        public virtual Subcategory Subcategory { get; private set; }
    }
}
