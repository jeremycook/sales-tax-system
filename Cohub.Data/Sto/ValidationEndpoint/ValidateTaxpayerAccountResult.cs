using System.Runtime.Serialization;

#nullable disable

namespace Cohub.Data.Sto.ValidationEndpoint
{
    [DataContract(Namespace = "http://www.salestaxonline.com/RemoteAcctValidationSvc/")]
    public class ValidateTaxpayerAccountResult
    {
        /// <summary>
        /// If the request is valid returns "OK" and "FAIL" otherwise.
        /// </summary>
        [DataMember(Order = 1)]
        public string Code { get; set; }

        /// <summary>
        /// Invalid TaxAccountID: "Error: Authority Account Number (TaxID) not found."
        /// Invalid ReturnTypeCode: ""
        /// Invalid FilingFrequCode: "Error: Account is registered for a Filing Frequency of Monthly"
        /// </summary>
        [DataMember(Order = 2)]
        public string Detail { get; set; }

        [DataMember(Order = 3)]
        public string AdditionalInformation { get; set; }
    }
}
