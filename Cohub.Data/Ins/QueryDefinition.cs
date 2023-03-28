using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Ins
{
    public class QueryDefinition
    {
        public override string ToString()
        {
            return Id;
        }

        [Required]
        [Display(Name = "Query Name")]
        [RegularExpression("[a-z][a-z_]*", ErrorMessage = "The {0} field must start with a lowercase letter, and can only contain lowercase letters and underscores.")]
        public string Id { get; set; }

        [Required]
        [Display(Name = "SQL")]
        [DataType("Sql")]
        public string Sql { get; set; }
    }
}
