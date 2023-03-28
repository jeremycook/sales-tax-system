using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _7 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "has_filed",
                schema: "fin",
                table: "return",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
@"UPDATE fin.return
SET has_filed = true
WHERE EXISTS (SELECT * FROM fin.filing f WHERE f.return_id = return.id)");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "has_filed",
                schema: "fin",
                table: "return");
        }
    }
}
