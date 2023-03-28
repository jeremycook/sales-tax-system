using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "period_id_format",
                schema: "fin",
                table: "frequency",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "period_name_format",
                schema: "fin",
                table: "frequency",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "period_id_format",
                schema: "fin",
                table: "frequency");

            migrationBuilder.DropColumn(
                name: "period_name_format",
                schema: "fin",
                table: "frequency");
        }
    }
}
