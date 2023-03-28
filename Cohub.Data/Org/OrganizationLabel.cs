using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Org
{
    public class OrganizationLabel
    {
        public OrganizationLabel()
        {
        }

        public OrganizationLabel(OrganizationLabel input)
        {
            input.OrganizationId = input.OrganizationId;
            UpdateWith(input);
        }

        public void UpdateWith(OrganizationLabel input)
        {
            LabelId = input.LabelId;
        }

        public override string ToString()
        {
            return LabelId;
        }

        [Required]
        public string OrganizationId { get; set; }

        [Required]
        public string LabelId { get; set; }

        public DateTimeOffset Created { get; set; }

        public virtual Label Label { get; private set; }
        public virtual Organization Organization { get; private set; }
    }
}
