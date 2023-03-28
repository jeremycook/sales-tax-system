using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public abstract class Filing
    {
        protected Filing(string typeId)
        {
            TypeId = typeId;
        }

        protected Filing(string typeId, DateTimeOffset created)
        {
            TypeId = typeId;
            Created = created;
            Updated = created;
        }

        public override string ToString()
        {
            return $"Filed {FilingDate:d} for {Return}";
        }

        public abstract void UpdateWith(Filing input);

        /// <summary>
        /// Instantiate a new concrete filing for the <paramref name="return"/>.
        /// </summary>
        /// <param name="return"></param>
        /// <returns></returns>
        public static Filing Activate(Return @return)
        {
            Filing filing;

            switch (@return.Category.TypeId)
            {
                case CategoryTypeId.Assessment:
                    filing = new AssessmentFiling
                    {
                        ReturnId = @return.Id,
                        FilingDate = @return.Period.DueDate,
                    };
                    break;
                case CategoryTypeId.Fee:
                    filing = new FeeFiling
                    {
                        ReturnId = @return.Id,
                        FilingDate = @return.Period.DueDate,
                    };
                    break;
                case CategoryTypeId.Tax:
                    filing = new TaxFiling
                    {
                        ReturnId = @return.Id,
                        FilingDate = @return.Period.DueDate,
                    };
                    break;
                default:
                    throw new NotSupportedException(@return.Category.TypeId.ToString());
            }

            return filing;
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [ScaffoldColumn(false)]
        public string TypeId { get; private set; }

        [Editable(false)]
        public int ReturnId { get; set; }
        public virtual Return Return { get; private set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FilingDate { get; set; }

        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Updated { get; private set; }
    }
}
