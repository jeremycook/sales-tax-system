using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace Cohub.WebApp.Areas.Fin.Views.ReturnReport
{
    public class PrintModel
    {
        public IEnumerable<PrintRecordModel> Records { get; set; } = Enumerable.Empty<PrintRecordModel>();

        public class PrintRecordModel
        {
            public string CategoryId { get; set; } = null!;
            public string PeriodId { get; set; } = null!;
            [DataType(DataType.Date)]
            public DateTime DueDate { get; set; }
            public string OrganizationId { get; set; } = null!;
            public string OrganizationName { get; set; } = null!;
            public string? Dba { get; set; }
            public string PhysicalAddress { get; set; } = null!;
            public string OrganizationPhone { get; set; } = null!;
            public string? OrganizationFax { get; set; }
        }
    }
}
