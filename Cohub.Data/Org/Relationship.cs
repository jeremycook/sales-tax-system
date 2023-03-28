using System;
using System.Collections.Generic;

#nullable disable

namespace Cohub.Data.Org
{
    public class Relationship
    {
        public Relationship()
        {
            OrganizationContacts = new HashSet<OrganizationContact>();
        }

        public override string ToString()
        {
            return Id;
        }

        public string Id { get; set; }
        public bool IsActive { get; set; }
        public string Description { get; set; }
        public DateTimeOffset Created { get; private set; }

        public virtual ICollection<OrganizationContact> OrganizationContacts { get; set; }
    }
}
