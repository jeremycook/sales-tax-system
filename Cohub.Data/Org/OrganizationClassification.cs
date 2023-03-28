using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Org
{
    public class OrganizationClassification
    {
        public OrganizationClassification()
        {
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
