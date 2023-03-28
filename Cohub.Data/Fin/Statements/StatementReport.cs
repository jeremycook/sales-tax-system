using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

#nullable disable

namespace Cohub.Data.Fin.Statements
{
    public class StatementReport
    {
        public StatementTypeId TypeId { get; set; }

        public DateTime NoticeDate { get; set; }
        public DateTime? AssessmentDueDate { get; set; }

        public string OrganizationId { get; set; }
        public string OrganizationName { get; set; }
#nullable enable
        public string? Dba { get; set; }
#nullable disable
        public string MulilineAddress { get; set; }

        public List<StatementReportScheduleItem> Schedule { get; set; } = new List<StatementReportScheduleItem>();

        public class StatementReportScheduleItem
        {
            public DateTime DueDate { get; set; }

            public string PeriodCovered { get; set; }

            public string Category { get; set; }

            [DataType(DataType.Currency)]
            public decimal? Net { get; set; }

            [DataType(DataType.Currency)]
            public decimal? Penalty { get; set; }

            [DataType(DataType.Currency)]
            public decimal? Interest { get; set; }

            [DataType(DataType.Currency)]
            public decimal TotalDue { get; set; }

            [DataType(DataType.Currency)]
            public decimal TotalOverpayment { get; set; }

            public string ReasonCode { get; set; }
        }
    }
}
