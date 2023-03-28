using Cohub.Data.Usr;
using System;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Ins
{
    public class Report
    {
        public Report()
        {
        }

        public Report(Report input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Report input)
        {
            Name = input.Name;
            QueryDefinitionId = input.QueryDefinitionId;
            QueryDefinition = input.QueryDefinition;
            Template = input.Template;
        }

        public override string ToString()
        {
            return Name;
        }

        public int Id { get; set; }

        [Required]
        public string Name { get; set; }

        [Required]
        public string QueryDefinitionId { get; set; }
        public virtual QueryDefinition QueryDefinition { get; private set; }

        [Required]
        [DataType("Liquid")]
        public string Template { get; set; }

        public DateTimeOffset Created { get; private set; }
        public int CreatedById { get; private set; }
        public virtual User CreatedBy { get; private set; }
    }
}
