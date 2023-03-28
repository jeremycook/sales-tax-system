using System.Runtime.Serialization;

#nullable disable

namespace Cohub.Data.Sto.ValidationEndpoint
{
    [DataContract(Namespace = "http://www.salestaxonline.com/RemoteAcctValidationSvc/")]
    public class ValidateTaxpayerReturnResult
    {
        /// <summary>
        /// If the request is valid returns "OK" and "FAIL" otherwise.
        /// </summary>
        /// <remarks>
        /// It seems like FilingPeriod is ignored because the observed behavior of an 
        /// invalid FilingPeriod is that the result is OK as long as the other arguments are valid.
        /// </remarks>
        [DataMember(Order = 1)]
        public string Code { get; set; }

        /// <summary>
        /// Invalid TaxAccountID: "Error: Authority Account Number (TaxID) not found."
        /// Invalid ReturnTypeCode: ""
        /// Invalid FilingFreqCode: "Error: Account is registered for a Filing Frequency of Monthly"
        /// Invalid FilingPeriod: ""
        /// </summary>
        [DataMember(Order = 2)]
        public string Detail { get; set; }

        [DataMember(Order = 3)]
        public string AdditionalInformation { get; set; }
    }
}
