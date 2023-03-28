using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Usr
{
    public static class Policy
    {
        public static readonly Dictionary<string, IReadOnlyList<string>> Roles = new()
        {
            [Super] = new[] { RoleId.Super },
            [Manage] = new[] { RoleId.Super, RoleId.Manager },
            [Process] = new[] { RoleId.Super, RoleId.Manager, RoleId.Processor },
            [Audit] = new[] { RoleId.Super, RoleId.Manager, RoleId.Processor, RoleId.Auditor },
            [Review] = new[] { RoleId.Super, RoleId.Manager, RoleId.Processor, RoleId.Auditor, RoleId.Reviewer },
            [ReviewLicenses] = new[] { RoleId.Super, RoleId.Manager, RoleId.Processor, RoleId.Auditor, RoleId.Reviewer, RoleId.LicenseReviewer },
            [Internal] = new[] { RoleId.Admin, RoleId.Internal, RoleId.Super, RoleId.Manager, RoleId.Processor, RoleId.Auditor, RoleId.Reviewer, RoleId.LicenseReviewer },
        };

        [Display(Description = "All privileges.")]
        public const string Super = "Super";
        [Display(Description = "All Process privileges plus able to configure sales tax settings.")]
        public const string Manage = "Manage";
        [Display(Description = "All Audit privileges plus able to process sales tax data.")]
        public const string Process = "Process";
        [Display(Description = "All Review privileges plus able to make organization comments, and manage statements.")]
        public const string Audit = "Audit";
        [Display(Description = "Able to view business and sales tax data.")]
        public const string Review = "Review";
        [Display(Description = "Able to view business data.")]
        public const string ReviewLicenses = "LicenseReview";
        public const string Internal = "Internal";
    }
}
