using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _15 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "role_id",
                schema: "usr",
                table: "user",
                type: "text",
                nullable: false,
                defaultValueSql: "'Disabled'");

            migrationBuilder.Sql(
@"UPDATE usr.user u
SET role_id = ur.role_id
FROM usr.user_role ur
WHERE ur.user_id = u.id");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "usr");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_id",
                schema: "usr",
                table: "user",
                column: "role_id");

            migrationBuilder.AddForeignKey(
                name: "FK_user_role_role_id",
                schema: "usr",
                table: "user",
                column: "role_id",
                principalSchema: "usr",
                principalTable: "role",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "user_role",
                schema: "usr",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    role_id = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_role", x => new { x.user_id, x.role_id });
                    table.ForeignKey(
                        name: "FK_user_role_role_role_id",
                        column: x => x.role_id,
                        principalSchema: "usr",
                        principalTable: "role",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_role_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
@"INSERT into usr.user_role (user_id, role_id, created)
SELECT u.id, u.role_id, u.created
FROM usr.user u");

            migrationBuilder.DropForeignKey(
                name: "FK_user_role_role_id",
                schema: "usr",
                table: "user");

            migrationBuilder.DropIndex(
                name: "IX_user_role_id",
                schema: "usr",
                table: "user");

            migrationBuilder.DropColumn(
                name: "role_id",
                schema: "usr",
                table: "user");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_role_id",
                schema: "usr",
                table: "user_role",
                column: "role_id");
        }
    }
}
