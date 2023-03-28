using Microsoft.EntityFrameworkCore.Migrations;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _20 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "sts");

            migrationBuilder.RenameTable(
                name: "user_mention",
                schema: "usr",
                newName: "user_mention",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "user_login",
                schema: "usr",
                newName: "user_login",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "user",
                schema: "usr",
                newName: "user",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "tz",
                schema: "geo",
                newName: "tz",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "transaction_detail",
                schema: "fin",
                newName: "transaction_detail",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "transaction",
                schema: "fin",
                newName: "transaction",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "subcategory",
                schema: "fin",
                newName: "subcategory",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "statement_reason_code",
                schema: "fin",
                newName: "statement_reason_code",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "statement_due",
                schema: "fin",
                newName: "statement_due",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "statement_comment",
                schema: "fin",
                newName: "statement_comment",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "statement",
                schema: "fin",
                newName: "statement",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "state",
                schema: "geo",
                newName: "state",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "role",
                schema: "usr",
                newName: "role",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "return_status",
                schema: "fin",
                newName: "return_status",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "return_label",
                schema: "fin",
                newName: "return_label",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "return_comment",
                schema: "fin",
                newName: "return_comment",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "return",
                schema: "fin",
                newName: "return",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "report",
                schema: "ins",
                newName: "report",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "relationship",
                schema: "org",
                newName: "relationship",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "period",
                schema: "fin",
                newName: "period",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "payment_configuration",
                schema: "fin",
                newName: "payment_configuration",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "payment_chart",
                schema: "fin",
                newName: "payment_chart",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization_type",
                schema: "org",
                newName: "organization_type",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization_status",
                schema: "org",
                newName: "organization_status",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization_label",
                schema: "org",
                newName: "organization_label",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization_contact",
                schema: "org",
                newName: "organization_contact",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization_comment",
                schema: "org",
                newName: "organization_comment",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization_classification",
                schema: "org",
                newName: "organization_classification",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "organization",
                schema: "org",
                newName: "organization",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "locale",
                schema: "geo",
                newName: "locale",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "license_type",
                schema: "lic",
                newName: "license_type",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "license",
                schema: "lic",
                newName: "license",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "label",
                schema: "org",
                newName: "label",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "frequency",
                schema: "fin",
                newName: "frequency",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "filing_schedule",
                schema: "fin",
                newName: "filing_schedule",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "filing",
                schema: "fin",
                newName: "filing",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "document",
                schema: "usr",
                newName: "document",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "comment",
                schema: "usr",
                newName: "comment",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "category",
                schema: "fin",
                newName: "category",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "bucket",
                schema: "fin",
                newName: "bucket",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "batch_comment",
                schema: "fin",
                newName: "batch_comment",
                newSchema: "sts");

            migrationBuilder.RenameTable(
                name: "batch",
                schema: "fin",
                newName: "batch",
                newSchema: "sts");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fin");

            migrationBuilder.EnsureSchema(
                name: "usr");

            migrationBuilder.EnsureSchema(
                name: "org");

            migrationBuilder.EnsureSchema(
                name: "lic");

            migrationBuilder.EnsureSchema(
                name: "geo");

            migrationBuilder.EnsureSchema(
                name: "ins");

            migrationBuilder.RenameTable(
                name: "user_mention",
                schema: "sts",
                newName: "user_mention",
                newSchema: "usr");

            migrationBuilder.RenameTable(
                name: "user_login",
                schema: "sts",
                newName: "user_login",
                newSchema: "usr");

            migrationBuilder.RenameTable(
                name: "user",
                schema: "sts",
                newName: "user",
                newSchema: "usr");

            migrationBuilder.RenameTable(
                name: "tz",
                schema: "sts",
                newName: "tz",
                newSchema: "geo");

            migrationBuilder.RenameTable(
                name: "transaction_detail",
                schema: "sts",
                newName: "transaction_detail",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "transaction",
                schema: "sts",
                newName: "transaction",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "subcategory",
                schema: "sts",
                newName: "subcategory",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "statement_reason_code",
                schema: "sts",
                newName: "statement_reason_code",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "statement_due",
                schema: "sts",
                newName: "statement_due",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "statement_comment",
                schema: "sts",
                newName: "statement_comment",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "statement",
                schema: "sts",
                newName: "statement",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "state",
                schema: "sts",
                newName: "state",
                newSchema: "geo");

            migrationBuilder.RenameTable(
                name: "role",
                schema: "sts",
                newName: "role",
                newSchema: "usr");

            migrationBuilder.RenameTable(
                name: "return_status",
                schema: "sts",
                newName: "return_status",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "return_label",
                schema: "sts",
                newName: "return_label",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "return_comment",
                schema: "sts",
                newName: "return_comment",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "return",
                schema: "sts",
                newName: "return",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "report",
                schema: "sts",
                newName: "report",
                newSchema: "ins");

            migrationBuilder.RenameTable(
                name: "relationship",
                schema: "sts",
                newName: "relationship",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "period",
                schema: "sts",
                newName: "period",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "payment_configuration",
                schema: "sts",
                newName: "payment_configuration",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "payment_chart",
                schema: "sts",
                newName: "payment_chart",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "organization_type",
                schema: "sts",
                newName: "organization_type",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "organization_status",
                schema: "sts",
                newName: "organization_status",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "organization_label",
                schema: "sts",
                newName: "organization_label",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "organization_contact",
                schema: "sts",
                newName: "organization_contact",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "organization_comment",
                schema: "sts",
                newName: "organization_comment",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "organization_classification",
                schema: "sts",
                newName: "organization_classification",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "organization",
                schema: "sts",
                newName: "organization",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "locale",
                schema: "sts",
                newName: "locale",
                newSchema: "geo");

            migrationBuilder.RenameTable(
                name: "license_type",
                schema: "sts",
                newName: "license_type",
                newSchema: "lic");

            migrationBuilder.RenameTable(
                name: "license",
                schema: "sts",
                newName: "license",
                newSchema: "lic");

            migrationBuilder.RenameTable(
                name: "label",
                schema: "sts",
                newName: "label",
                newSchema: "org");

            migrationBuilder.RenameTable(
                name: "frequency",
                schema: "sts",
                newName: "frequency",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "filing_schedule",
                schema: "sts",
                newName: "filing_schedule",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "filing",
                schema: "sts",
                newName: "filing",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "document",
                schema: "sts",
                newName: "document",
                newSchema: "usr");

            migrationBuilder.RenameTable(
                name: "comment",
                schema: "sts",
                newName: "comment",
                newSchema: "usr");

            migrationBuilder.RenameTable(
                name: "category",
                schema: "sts",
                newName: "category",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "bucket",
                schema: "sts",
                newName: "bucket",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "batch_comment",
                schema: "sts",
                newName: "batch_comment",
                newSchema: "fin");

            migrationBuilder.RenameTable(
                name: "batch",
                schema: "sts",
                newName: "batch",
                newSchema: "fin");
        }
    }
}
