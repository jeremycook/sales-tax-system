using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _4 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_report_query_definition_query_definition_id",
                schema: "ins",
                table: "report");

            migrationBuilder.DropForeignKey(
                name: "FK_statement_organization_organization_id",
                schema: "fin",
                table: "statement");

            migrationBuilder.DropTable(
                name: "query_definition",
                schema: "ins");

            migrationBuilder.DropTable(
                name: "statement_detail",
                schema: "fin");

            migrationBuilder.DropColumn(
                name: "is_assessment",
                schema: "fin",
                table: "statement");

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                schema: "fin",
                table: "statement",
                type: "character varying(25)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(25)",
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "grand_total_due",
                schema: "fin",
                table: "statement",
                type: "numeric",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "overpayment_balance",
                schema: "fin",
                table: "statement",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "type_id",
                schema: "fin",
                table: "statement",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "updated",
                schema: "fin",
                table: "statement",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTimeOffset(new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new TimeSpan(0, 0, 0, 0, 0)));

            migrationBuilder.CreateTable(
                name: "statement_due",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    statement_id = table.Column<int>(type: "integer", nullable: false),
                    category_id = table.Column<string>(type: "text", nullable: false),
                    period_id = table.Column<string>(type: "text", nullable: false),
                    has_filed = table.Column<bool>(type: "boolean", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: false),
                    net_due = table.Column<decimal>(type: "numeric", nullable: false),
                    penalty_due = table.Column<decimal>(type: "numeric", nullable: false),
                    interest_due = table.Column<decimal>(type: "numeric", nullable: false),
                    total_due = table.Column<decimal>(type: "numeric", nullable: false),
                    bucket_id = table.Column<string>(type: "text", nullable: true),
                    return_id = table.Column<int>(type: "integer", nullable: true),
                    statement_reason_code_id = table.Column<string>(type: "text", nullable: true),
                    subcategory_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statement_due", x => x.id);
                    table.ForeignKey(
                        name: "FK_statement_due_bucket_bucket_id",
                        column: x => x.bucket_id,
                        principalSchema: "fin",
                        principalTable: "bucket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_statement_due_period_period_id",
                        column: x => x.period_id,
                        principalSchema: "fin",
                        principalTable: "period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_statement_due_return_return_id",
                        column: x => x.return_id,
                        principalSchema: "fin",
                        principalTable: "return",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_statement_due_statement_reason_code_statement_reason_code_id",
                        column: x => x.statement_reason_code_id,
                        principalSchema: "fin",
                        principalTable: "statement_reason_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_statement_due_statement_statement_id",
                        column: x => x.statement_id,
                        principalSchema: "fin",
                        principalTable: "statement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_statement_due_subcategory_subcategory_id",
                        column: x => x.subcategory_id,
                        principalSchema: "fin",
                        principalTable: "subcategory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_statement_due_bucket_id",
                schema: "fin",
                table: "statement_due",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_due_period_id",
                schema: "fin",
                table: "statement_due",
                column: "period_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_due_return_id",
                schema: "fin",
                table: "statement_due",
                column: "return_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_due_statement_id",
                schema: "fin",
                table: "statement_due",
                column: "statement_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_due_statement_reason_code_id",
                schema: "fin",
                table: "statement_due",
                column: "statement_reason_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_due_subcategory_id",
                schema: "fin",
                table: "statement_due",
                column: "subcategory_id");

            migrationBuilder.AddForeignKey(
                name: "FK_statement_organization_organization_id",
                schema: "fin",
                table: "statement",
                column: "organization_id",
                principalSchema: "org",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_statement_organization_organization_id",
                schema: "fin",
                table: "statement");

            migrationBuilder.DropTable(
                name: "statement_due",
                schema: "fin");

            migrationBuilder.DropColumn(
                name: "grand_total_due",
                schema: "fin",
                table: "statement");

            migrationBuilder.DropColumn(
                name: "overpayment_balance",
                schema: "fin",
                table: "statement");

            migrationBuilder.DropColumn(
                name: "type_id",
                schema: "fin",
                table: "statement");

            migrationBuilder.DropColumn(
                name: "updated",
                schema: "fin",
                table: "statement");

            migrationBuilder.AlterColumn<string>(
                name: "organization_id",
                schema: "fin",
                table: "statement",
                type: "character varying(25)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(25)");

            migrationBuilder.AddColumn<bool>(
                name: "is_assessment",
                schema: "fin",
                table: "statement",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "query_definition",
                schema: "ins",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    sql = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_query_definition", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "statement_detail",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    bucket_id = table.Column<string>(type: "text", nullable: true),
                    fees_due = table.Column<double>(type: "double precision", nullable: false),
                    interest_due = table.Column<double>(type: "double precision", nullable: false),
                    nsf_fees_due = table.Column<double>(type: "double precision", nullable: false),
                    penalty_due = table.Column<double>(type: "double precision", nullable: false),
                    period_id = table.Column<string>(type: "text", nullable: true),
                    reason_code_id = table.Column<string>(type: "text", nullable: true),
                    return_id = table.Column<int>(type: "integer", nullable: false),
                    statement_id = table.Column<int>(type: "integer", nullable: false),
                    subcategory_id = table.Column<string>(type: "text", nullable: true),
                    tax_due = table.Column<double>(type: "double precision", nullable: false),
                    total_due = table.Column<double>(type: "double precision", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statement_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_statement_detail_bucket_bucket_id",
                        column: x => x.bucket_id,
                        principalSchema: "fin",
                        principalTable: "bucket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_statement_detail_period_period_id",
                        column: x => x.period_id,
                        principalSchema: "fin",
                        principalTable: "period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_statement_detail_return_return_id",
                        column: x => x.return_id,
                        principalSchema: "fin",
                        principalTable: "return",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_statement_detail_statement_reason_code_reason_code_id",
                        column: x => x.reason_code_id,
                        principalSchema: "fin",
                        principalTable: "statement_reason_code",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_statement_detail_statement_statement_id",
                        column: x => x.statement_id,
                        principalSchema: "fin",
                        principalTable: "statement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_statement_detail_subcategory_subcategory_id",
                        column: x => x.subcategory_id,
                        principalSchema: "fin",
                        principalTable: "subcategory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_statement_detail_bucket_id",
                schema: "fin",
                table: "statement_detail",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_detail_period_id",
                schema: "fin",
                table: "statement_detail",
                column: "period_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_detail_reason_code_id",
                schema: "fin",
                table: "statement_detail",
                column: "reason_code_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_detail_return_id",
                schema: "fin",
                table: "statement_detail",
                column: "return_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_detail_statement_id",
                schema: "fin",
                table: "statement_detail",
                column: "statement_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_detail_subcategory_id",
                schema: "fin",
                table: "statement_detail",
                column: "subcategory_id");

            migrationBuilder.AddForeignKey(
                name: "FK_report_query_definition_query_definition_id",
                schema: "ins",
                table: "report",
                column: "query_definition_id",
                principalSchema: "ins",
                principalTable: "query_definition",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_statement_organization_organization_id",
                schema: "fin",
                table: "statement",
                column: "organization_id",
                principalSchema: "org",
                principalTable: "organization",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
