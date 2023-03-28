using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _10 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_filing_filing_date",
                schema: "fin",
                table: "filing",
                column: "filing_date");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_filing_filing_date",
                schema: "fin",
                table: "filing");
        }
    }
}
