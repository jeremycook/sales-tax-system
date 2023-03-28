namespace Cohub.Data.Configuration
{
    public class CohubDataOptions
    {
        public string OwnerRole { get; set; } = "anywhereusa_prod_owner";
        /// <summary>
        /// Defaults to "cohub_reader".
        /// </summary>
        public string ReadRole { get; set; } = "cohub_reader";
    }
}
