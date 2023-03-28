using System;

namespace SiteKit.Collections
{
    public class Paging
    {
        public static int DefaultRecordPerPage { get; set; } = 24;

        public Paging() { }
        public Paging(int pageNumber, int recordsPerPage, int totalRecords)
        {
            PageNumber = pageNumber;
            RecordsPerPage = recordsPerPage;
            TotalRecords = totalRecords;
        }

        public int PageNumber { get; set; } = 1;
        public int RecordsPerPage { get; set; } = DefaultRecordPerPage;
        public int TotalRecords { get; set; }


        public int Index => (PageNumber - 1) * RecordsPerPage;
        public int TotalPages => (int)Math.Ceiling((float)TotalRecords / RecordsPerPage);
    }
}
