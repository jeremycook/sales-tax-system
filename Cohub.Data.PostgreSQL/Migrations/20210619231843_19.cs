using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _19 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organization_comment_organization_organization_id",
                schema: "org",
                table: "organization_comment");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_comment_organization_organization_id",
                schema: "org",
                table: "organization_comment",
                column: "organization_id",
                principalSchema: "org",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_organization_comment_organization_organization_id",
                schema: "org",
                table: "organization_comment");

            migrationBuilder.AddForeignKey(
                name: "FK_organization_comment_organization_organization_id",
                schema: "org",
                table: "organization_comment",
                column: "organization_id",
                principalSchema: "org",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
