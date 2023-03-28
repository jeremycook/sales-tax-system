using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Usr
{
    public class Role : IEquatable<Role>
    {
        public override string ToString()
        {
            return Id;
        }

        public override bool Equals(object obj)
        {
            return Equals(obj as Role);
        }

        public bool Equals(Role other)
        {
            return other != null &&
                   Id == other.Id;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Id);
        }

        [Required]
        public string Id { get; set; }

        public bool IsActive { get; set; }

        public string Color { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public virtual List<User> Users { get; set; } = new();

        public static bool operator ==(Role left, Role right)
        {
            return EqualityComparer<Role>.Default.Equals(left, right);
        }

        public static bool operator !=(Role left, Role right)
        {
            return !(left == right);
        }
    }
}
