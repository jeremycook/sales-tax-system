using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Cohub.Data.Fin.Deposits
{
    public class DepositInfo : IValidatableObject
    {
        [Required]
        [DataType("BatchId")]
        public int BatchId { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DefaultDepositDate { get; set; }

        public List<Deposit> Deposits { get; set; } = new List<Deposit>();

        public bool AllowMissingFilings { get; set; } = false;

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            var db = (CohubDbContext)validationContext.GetService(typeof(CohubDbContext))!;

            if (BatchId > 0)
            {
                var batch = db.Batches(BatchId).SingleOrDefault();
                if (batch == null)
                {
                    yield return new ValidationResult("The selected batch does not exist.", new[] { nameof(BatchId) });
                }
                else if (batch.IsPosted)
                {
                    yield return new ValidationResult("The selected batch is posted and cannot be modified.", new[] { nameof(BatchId) });
                }
            }
        }
    }
}
