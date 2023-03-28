using Microsoft.EntityFrameworkCore.Migrations;
using System;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _14 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "email",
                schema: "usr",
                table: "user",
                type: "text",
                nullable: true);

            migrationBuilder.Sql(
@"UPDATE usr.user u
SET email = ucm.value
FROM usr.user_contact_method ucm
WHERE ucm.user_id = u.id");

            migrationBuilder.Sql(
@"DELETE
FROM usr.user_login
WHERE issuer = 'https://constants.cohub.us/user-login-issuer/magic-email'");

            migrationBuilder.DropTable(
                name: "user_contact_method",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "contact_method_type",
                schema: "usr");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "contact_method_type",
                schema: "usr",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_contact_method_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "user_contact_method",
                schema: "usr",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_contact_method", x => new { x.user_id, x.type_id, x.value });
                    table.ForeignKey(
                        name: "FK_user_contact_method_contact_method_type_type_id",
                        column: x => x.type_id,
                        principalSchema: "usr",
                        principalTable: "contact_method_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_contact_method_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.Sql(
@"INSERT INTO usr.contact_method_type (id) VALUES ('Email')");

            migrationBuilder.Sql(
@"INSERT into usr.user_contact_method (user_id, type_id, value, is_verified, created)
SELECT u.id, 'Email', u.email, true, u.created
FROM usr.user u
WHERE length(u.email) > 0");

            migrationBuilder.DropColumn(
                name: "email",
                schema: "usr",
                table: "user");

            migrationBuilder.CreateIndex(
                name: "IX_user_contact_method_type_id",
                schema: "usr",
                table: "user_contact_method",
                column: "type_id");
        }
    }
}
