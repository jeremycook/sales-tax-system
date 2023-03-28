using System;
using System.ComponentModel.DataAnnotations;

namespace Cohub.Data.Pg
{
    public class TableId
    {
        public override string ToString()
        {
            return $"{TableSchema}.{TableName}";
        }

        [Required]
        public string? TableSchema { get; set; }
        [Required]
        public string? TableName { get; set; }
    }
}
