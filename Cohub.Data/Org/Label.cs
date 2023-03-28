using SiteKit.ComponentModel;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text.RegularExpressions;

#nullable disable

namespace Cohub.Data.Org
{
    public class Label
    {
        private string _Id;

        public Label()
        {
        }

        public Label(DateTimeOffset created)
        {
            Created = created;
        }

        public Label(Label input)
        {
            Id = input.Id;
            UpdateWith(input);
        }

        public void UpdateWith(Label input)
        {
            IsActive = input.IsActive;
            Title = input.Title;
            Description = input.Description;
        }

        public override string ToString()
        {
            return Id;
        }

        [Required]
        [RegularExpression(".+:.+", ErrorMessage = "The {0} field must contain a colon that separates the label's group from its value.")]
        public string Id
        {
            get => _Id;
            set
            {
                if (string.IsNullOrWhiteSpace(value) || value.Trim() == ":")
                {
                    _Id = null;
                    GroupId = null;
                    Value = null;
                }
                else if (value.Contains(":"))
                {
                    _Id = Regex.Replace(value, " +", " ");

                    string[] split = _Id.Split(":", 2, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim()).ToArray();

                    GroupId = split[0];
                    Value = split[1];
                    Title ??= Value;
                }
                else
                {
                    _Id = null;
                    GroupId = null;
                    Value = Regex.Replace(value, " +", " ").Trim();
                }
            }
        }

        [Boolean("Active", "Inactive")]
        public bool IsActive { get; set; }

        [Required]
        public string GroupId { get; private set; }

        [Required]
        public string Value { get; private set; }

        [Required]
        public string Title { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public DateTimeOffset Created { get; private set; }
        public int CreatedById { get; private set; }

        public DateTimeOffset Updated { get; private set; }
        public int UpdatedById { get; private set; }

        public virtual Usr.User CreatedBy { get; private set; }
        public virtual Usr.User UpdatedBy { get; private set; }

        public virtual IReadOnlyList<Organization> Organizations { get; private set; }
        public virtual IReadOnlyList<Fin.Return> Returns { get; private set; }
    }
}
