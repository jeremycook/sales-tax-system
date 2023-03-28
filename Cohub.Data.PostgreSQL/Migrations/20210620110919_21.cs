using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _21 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "license_settings",
                schema: "sts",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    current_business_license_expiration_date = table.Column<DateTime>(type: "date", nullable: false),
                    next_business_license_expiration_date = table.Column<DateTime>(type: "date", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_settings", x => x.id);
                    table.ForeignKey(
                        name: "FK_license_settings_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "sts",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_license_settings_created_by_id",
                schema: "sts",
                table: "license_settings",
                column: "created_by_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "license_settings",
                schema: "sts");
        }
    }
}
