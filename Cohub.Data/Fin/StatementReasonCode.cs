using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class StatementReasonCode
    {
        public StatementReasonCode()
        {
        }

        public StatementReasonCode(DateTimeOffset created)
        {
            Created = created;
        }

        public override string ToString()
        {
            return Id;
        }

        [Required]
        public string Id { get; set; }

        public bool IsActive { get; set; }

        [Required]
        public string Name { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual IReadOnlyList<StatementDue> StatementDetails { get; private set; }
    }
}
