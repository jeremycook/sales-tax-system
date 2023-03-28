using Microsoft.EntityFrameworkCore.Metadata;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Generator.Views.ScaffoldSubclasses
{
    public record ScaffoldByTypeInput
    {
        [Required]
        public string SourceTypeFullNamePattern { get; set; } = string.Empty;
        public string? DestinationDirectory { get; set; }
        public bool Preview { get; set; } = true;

        public IEntityType? EntityType { get; private set; }

        public void SetEntityType(IEntityType entityType)
        {
            EntityType = entityType;
        }
    }
}
