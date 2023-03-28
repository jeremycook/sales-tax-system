using System;
using System.Collections.Generic;

namespace Cohub.Data.Lic
{
    public class LicenseType
    {
        public LicenseType()
        {
        }

        public LicenseType(DateTimeOffset created)
        {
            Created = created;
        }

        public LicenseType(LicenseType input)
        {
            Id = input.Id;
            IsActive = input.IsActive;
            Description = input.Description;
        }

        public void UpdateWith(LicenseType input)
        {
            Id = input.Id;
            IsActive = input.IsActive;
            Description = input.Description;
        }

        public override string ToString()
        {
            return Id;
        }

        public string Id { get; set; } = default!;
        public bool IsActive { get; set; } = default!;
        public string? Description { get; set; }
        public DateTimeOffset Created { get; private set; }

        public virtual IReadOnlyList<License>? Licenses { get; private set; }
    }
}
