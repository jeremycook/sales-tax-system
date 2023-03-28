using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Lic
{
    public class LicenseSettings : IEquatable<LicenseSettings?>
    {
        public override string ToString()
        {
            return "License Settings";
        }

        public bool Equals(LicenseSettings? other)
        {
            return other != null &&
                   CurrentBusinessLicenseExpirationDate == other.CurrentBusinessLicenseExpirationDate &&
                   NextBusinessLicenseExpirationDate == other.NextBusinessLicenseExpirationDate;
        }

        [ScaffoldColumn(false)]
        public int Id { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CurrentBusinessLicenseExpirationDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime NextBusinessLicenseExpirationDate { get; set; }

        [ScaffoldColumn(false)]
        public DateTimeOffset Created { get; private set; }
        [ScaffoldColumn(false)]
        public int CreatedById { get; private set; }

        [ScaffoldColumn(false)]
        public virtual Usr.User? CreatedBy { get; private set; }

        public override bool Equals(object? obj)
        {
            return Equals(obj as LicenseSettings);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(CurrentBusinessLicenseExpirationDate, NextBusinessLicenseExpirationDate);
        }

        public static bool operator ==(LicenseSettings? left, LicenseSettings? right)
        {
            return EqualityComparer<LicenseSettings>.Default.Equals(left, right);
        }

        public static bool operator !=(LicenseSettings? left, LicenseSettings? right)
        {
            return !(left == right);
        }
    }
}
