using Cohub.Data.Geo;
using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Org
{
    public class OrganizationContact
    {
        public OrganizationContact()
        {
        }

        public OrganizationContact(OrganizationContact input)
        {
            UpdateWith(input);
        }

        public void UpdateWith(OrganizationContact input)
        {
            OrganizationId = input.OrganizationId;
            LegalName = input.LegalName;
            RelationshipId = input.RelationshipId;
            Email = input.Email;
            PhoneNumber = input.PhoneNumber;
            Address.UpdateWith(input.Address);
        }

        public override string ToString()
        {
            return LegalName;
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        public string LegalName { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string OrganizationId { get; set; }

        [Required]
        [DataType("RelationshipId")]
        public string RelationshipId { get; set; }

        [EmailAddress]
        public string Email { get; set; }

        [Phone]
        public string PhoneNumber { get; set; }

        public virtual Address Address { get; set; } = new Address();

        public DateTimeOffset Created { get; private set; }

        public virtual Organization Organization { get; private set; }
        public virtual Relationship Relationship { get; private set; }
    }
}
