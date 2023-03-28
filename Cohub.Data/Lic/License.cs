using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Lic
{
    public class License
    {
        public License()
        {
        }

        public License(DateTimeOffset created)
        {
            Created = created;
        }

        public License(License input)
        {
            UpdateWith(input);
        }

        public void UpdateWith(License input)
        {
            OrganizationId = input.OrganizationId;
            Title = input.Title;
            TypeId = input.TypeId;
            IssuedDate = input.IssuedDate;
            ExpirationDate = input.ExpirationDate;
            Description = input.Description;
        }

        public override string ToString()
        {
            return Title;
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        public string Title { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string OrganizationId { get; set; }

        [Required]
        [DataType("LicenseTypeId")]
        public string TypeId { get; set; }

        [DataType(DataType.Date)]
        public DateTime IssuedDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime ExpirationDate { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Updated { get; private set; }

        public virtual Org.Organization Organization { get; private set; }
        public virtual LicenseType Type { get; private set; }
    }
}
