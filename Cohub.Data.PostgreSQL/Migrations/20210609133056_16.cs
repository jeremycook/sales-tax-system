using Cohub.Data.Org;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _16 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "online_filer",
                schema: "org",
                table: "organization",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            // All active or pending organizations should initially be marked as online filers
            migrationBuilder.Sql(
$@"UPDATE org.organization
SET online_filer = true
WHERE status_id IN ('{OrganizationStatusId.Active}', '{OrganizationStatusId.Pending}')");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "online_filer",
                schema: "org",
                table: "organization");
        }
    }
}
