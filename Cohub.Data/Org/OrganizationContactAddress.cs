using Cohub.Data.Geo;
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

#nullable disable

namespace Cohub.Data.Org
{
    public class OrganizationContactAddress
    {
        private string _fullAddress;
        private string _multilineAddress;

        public OrganizationContactAddress()
        {
        }
        public OrganizationContactAddress(OrganizationContactAddress input)
        {
            UpdateWith(input);
        }
        public void UpdateWith(OrganizationContactAddress organizationContactAddress)
        {
            AddressLines = organizationContactAddress.AddressLines;
            City = organizationContactAddress.City;
            StateId = organizationContactAddress.StateId;
            Zip = organizationContactAddress.Zip;
        }
        public static OrganizationContactAddress CloneAddress(Address address)
        {
            return new OrganizationContactAddress
            {
                AddressLines = address.AddressLines,
                City = address.City,
                StateId = address.StateId,
                Zip = address.Zip,
            };
        }
        public override string ToString()
        {
            return FullAddress;
        }

        [DataType("AddressLines")]
        public string AddressLines { get; set; }

        [DataType("AddressCity")]
        public string City { get; set; }

        [DataType("AddressStateId")]
        public string StateId { get; set; }

        [DataType("AddressZip")]
        public string Zip { get; set; }

        /// <summary>
        /// Single line address.
        /// </summary>
        public string FullAddress
        {
            get => _fullAddress = Regex.Replace((AddressLines != null ? AddressLines.Replace("\r", "") + ", " : null) + (City != null ? City + ", " : null) + StateId + " " + Zip, "[, \n]+$", "");
            private set => _fullAddress = value;
        }

        public string MultilineAddress
        {
            get => _multilineAddress = Regex.Replace((AddressLines != null ? AddressLines.Replace("\r", "") + "\n" : null) + (City != null ? City + ", " : null) + StateId + " " + Zip, "[, \n]+$", "");
            private set => _multilineAddress = value;
        }
    }
}
