using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class ReturnStatus
    {
        public static ReturnStatusId[] OpenIds { get; } = new[] { ReturnStatusId.Payable, ReturnStatusId.Due };

        public ReturnStatus()
        {
        }

        public ReturnStatus(DateTimeOffset created)
        {
            Created = created;
        }

        public override string ToString()
        {
            return Name;
        }

        [Required]
        public ReturnStatusId Id { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual IReadOnlyList<Return> Returns { get; private set; }
    }
}
