using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _18 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_report_user_created_by_id",
                schema: "ins",
                table: "report");

            migrationBuilder.DropTable(
                name: "report",
                schema: "rpt");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "rpt");

            migrationBuilder.CreateTable(
                name: "report",
                schema: "rpt",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    columns = table.Column<List<string>>(type: "jsonb", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_user_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_report_created_by_id1",
                schema: "rpt",
                table: "report",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_updated_by_id",
                schema: "rpt",
                table: "report",
                column: "updated_by_id");
        }
    }
}
