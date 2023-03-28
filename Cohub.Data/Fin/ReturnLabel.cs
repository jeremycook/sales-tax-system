using Cohub.Data.Org;
using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin
{
    public class ReturnLabel
    {
        public ReturnLabel()
        {
        }

        public ReturnLabel(ReturnLabel input)
        {
            input.ReturnId = input.ReturnId;
            UpdateWith(input);
        }

        public void UpdateWith(ReturnLabel input)
        {
            LabelId = input.LabelId;
        }

        public override string ToString()
        {
            return LabelId;
        }

        [Required]
        public int ReturnId { get; set; }

        [Required]
        public string LabelId { get; set; }

        public DateTimeOffset Created { get; set; }

        public virtual Label Label { get; private set; }
        public virtual Return Return { get; private set; }
    }
}
