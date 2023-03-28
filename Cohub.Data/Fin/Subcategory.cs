using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Subcategory
    {
        public Subcategory()
        {
        }

        public Subcategory(Subcategory input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Subcategory input)
        {
        }

        public override string ToString()
        {
            return Id;
        }

        [Required]
        public string Id { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual IReadOnlyList<StatementDue> StatementDetails { get; private set; }
        public virtual IReadOnlyList<TransactionDetail> TransactionDetails { get; private set; }
    }
}
