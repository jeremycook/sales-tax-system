using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Category
    {
        public Category()
        {
        }

        public Category(DateTimeOffset created)
        {
            Created = created;
        }

        public Category(Category input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Category input)
        {
            IsActive = input.IsActive;
            TypeId = input.TypeId;
            Description = input.Description;
        }

        public override string ToString()
        {
            return Id;
        }

        [Required]
        public string Id { get; set; }

        public bool IsActive { get; set; }

        public CategoryTypeId TypeId { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }

        public virtual IReadOnlyList<Return> Returns { get; private set; }
        public virtual IReadOnlyList<TransactionDetail> TransactionDetails { get; private set; }
    }
}
