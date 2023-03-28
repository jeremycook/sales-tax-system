using SiteKit.Data;
using System;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

#nullable disable

namespace Cohub.Data.Geo
{
    public class Address : IEmptiable
    {
        private string _addressLines;

        public Address()
        {
        }
        public Address(Address input)
        {
            UpdateWith(input);
        }
        public void UpdateWith(Address input)
        {
            AddressLines = input.AddressLines;
            City = input.City;
            StateId = input.StateId;
            Zip = input.Zip;
        }
        public override string ToString()
        {
            return FullAddress;
        }

        /// <summary>
        /// <see cref="AddressLines"/> converted to comma separated single line.
        /// </summary>
        public string GetSingleLineAddessLines()
        {
            return Regex.Replace(AddressLines != null ? Regex.Replace(AddressLines, @"\r?\n", ", ") + ", " : string.Empty, @"[,\s]+$", string.Empty);
        }

        [DataType("AddressLines")]
        public string AddressLines { get => _addressLines; set => _addressLines = value?.Nullify(); }

        [DataType("AddressCity")]
        public string City { get; set; }

        [DataType("AddressStateId")]
        public string StateId { get; set; }

        [DataType("AddressZip")]
        public string Zip { get; set; }

        /// <summary>
        /// Single line address.
        /// </summary>
        [ScaffoldColumn(false)]
        public string FullAddress
        {
            get => Regex.Replace((AddressLines != null ? Regex.Replace(AddressLines, @"\r?\n", ", ") + ", " : null) + (City != null ? City + ", " : null) + StateId + " " + Zip, @"[,\s]+$", "");
            private set { }
        }

        [ScaffoldColumn(false)]
        public string MultilineAddress
        {
            get => Regex.Replace((AddressLines != null ? Regex.Replace(AddressLines, @"\r", "") + "\n" : null) + (City != null ? City + ", " : null) + StateId + " " + Zip, @"[,\s]+$", "");
            private set { }
        }

        [ScaffoldColumn(false)]
        public bool IsEmpty
        {
            get => AddressLines is null && City is null && StateId is null && Zip is null;
            private set { }
        }
    }
}
