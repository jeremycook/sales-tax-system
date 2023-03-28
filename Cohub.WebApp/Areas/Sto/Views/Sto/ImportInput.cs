using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace Cohub.WebApp.Areas.Sto.Views.Sto
{
    public class ImportInput
    {
        [Required]
        [DataType("STOXMLFile")]
        [Display(Name = "STO XML File")]
        public IFormFile? STOXMLFile { get; set; }
    }
}
