using Cohub.Data.Org;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

#nullable disable

namespace Cohub.Data.Fin
{
    public class Return
    {
        private List<Filing> _filings;
        private List<Label> _labels;

        public CohubDbContext DbContext { get; private set; }

        public Return()
        {
        }
        public Return(DateTimeOffset created)
        {
            Created = created;
            Labels = new();
            Filings = new();
        }
        public Return(Return input)
        {
            Labels = new(input.Labels.OrEmptyEnumerable());
            Filings = new();
            UpdateWith(input);
        }
        public void UpdateWith(Return input)
        {
            StatusId = input.StatusId;
            OrganizationId = input.OrganizationId;
            PeriodId = input.PeriodId;
            CategoryId = input.CategoryId;
        }

        public bool IsOpen()
        {
            return ReturnStatus.OpenIds.Contains(StatusId);
        }

#nullable enable

        public Filing? GetLatestFiling()
        {
            return Filings.OrderBy(o => o.Created).ThenBy(o => o.Id).LastOrDefault();
        }

#nullable disable

        public override string ToString()
        {
            return $"{OrganizationId}: {PeriodId} {CategoryId}";
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        [DataType("ReturnStatusId")]
        public ReturnStatusId StatusId { get; set; }

        [Required]
        [DataType("OrganizationId")]
        public string OrganizationId { get; set; }

        [Required]
        [DataType("PeriodId")]
        public string PeriodId { get; set; }

        [Required]
        [DataType("CategoryId")]
        public string CategoryId { get; set; }

        public bool HasFiled { get; set; }

        [ScaffoldColumn(false)]
        public virtual List<Label> Labels { get => _labels ??= DbContext?.Set<ReturnLabel>().Where(o => o.ReturnId == Id).Select(o => o.Label).ToList(); set => _labels = value; }

        [ScaffoldColumn(false)]
        public virtual List<Filing> Filings { get => _filings ??= DbContext?.Filings().Where(o => o.ReturnId == Id).ToList(); set => _filings = value; }

        public DateTimeOffset Created { get; private set; }
        public DateTimeOffset Updated { get; private set; }

        public virtual ReturnStatus Status { get; private set; }
        public virtual Organization Organization { get; private set; }
        public virtual Period Period { get; private set; }
        public virtual Category Category { get; private set; }

        public virtual IReadOnlyList<ReturnComment> Comments { get; private set; }

        public virtual IReadOnlyList<StatementDue> StatementDetails { get; private set; }
    }
}
