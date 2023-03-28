using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Bucket
    {
        public Bucket()
        {
        }

        public Bucket(Bucket operation)
        {
            Id = operation.Id;
            UpdateWith(operation);
        }

        public void UpdateWith(Bucket operation)
        {
            Name = operation.Name;
            Description = operation.Description;
            IsActive = operation.IsActive;
        }

        public override string ToString()
        {
            return Name;
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
        public virtual IReadOnlyList<TransactionDetail> TransactionDetails { get; private set; }
    }
}
