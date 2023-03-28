using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _11 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_filing_created",
                schema: "fin",
                table: "filing",
                column: "created");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_filing_created",
                schema: "fin",
                table: "filing");
        }
    }
}
