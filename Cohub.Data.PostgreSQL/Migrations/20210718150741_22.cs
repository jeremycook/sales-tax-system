using Microsoft.EntityFrameworkCore.Migrations;
using SiteKit.Users;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _22 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "is_user_comment",
                schema: "sts",
                table: "comment",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql(
$@"UPDATE sts.comment
SET is_user_comment = true
WHERE author_id NOT IN ({StandardUserId.System}, {StandardUserId.Anonymous});"
            );

            migrationBuilder.CreateIndex(
                name: "IX_comment_is_user_comment",
                schema: "sts",
                table: "comment",
                column: "is_user_comment");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_comment_is_user_comment",
                schema: "sts",
                table: "comment");

            migrationBuilder.DropColumn(
                name: "is_user_comment",
                schema: "sts",
                table: "comment");
        }
    }
}
