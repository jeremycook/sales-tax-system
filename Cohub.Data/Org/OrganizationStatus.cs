using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Org
{
    public class OrganizationStatus
    {
        public OrganizationStatus()
        {
        }
        public OrganizationStatus(DateTimeOffset created)
        {
            Created = created;
        }
        public OrganizationStatus(OrganizationStatus input)
        {
            Id = input.Id;
            UpdateWith(input);
        }
        public void UpdateWith(OrganizationStatus input)
        {
            IsActive = input.IsActive;
            Description = input.Description;
        }
        public override string ToString()
        {
            return Id;
        }

        [Required]
        public string Id { get; set; }

        public bool IsActive { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual IReadOnlyList<Organization> Organizations { get; private set; }
    }
}
