using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

namespace Cohub.Data.PostgreSQL.Migrations
{
    public partial class _1 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "fin");

            migrationBuilder.EnsureSchema(
                name: "usr");

            migrationBuilder.EnsureSchema(
                name: "anywhereusa");

            migrationBuilder.EnsureSchema(
                name: "org");

            migrationBuilder.EnsureSchema(
                name: "lic");

            migrationBuilder.EnsureSchema(
                name: "geo");

            migrationBuilder.EnsureSchema(
                name: "ins");

            migrationBuilder.CreateTable(
                name: "bucket",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_bucket", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "category",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_category", x => x.id);
                });

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
                name: "frequency",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_frequency", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "license_type",
                schema: "lic",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license_type", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "locale",
                schema: "geo",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_locale", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_classification",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_classification", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_status",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "organization_type",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_type", x => x.id);
                });

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
                name: "relationship",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_relationship", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "return_status",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_return_status", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "role",
                schema: "usr",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    color = table.Column<string>(type: "text", nullable: true),
                    description = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "state",
                schema: "geo",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_state", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "statement_reason_code",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statement_reason_code", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "subcategory",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_subcategory", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "tz",
                schema: "geo",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_tz", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "period",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    frequency_id = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    due_date = table.Column<DateTime>(type: "date", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_period", x => x.id);
                    table.ForeignKey(
                        name: "FK_period_frequency_frequency_id",
                        column: x => x.frequency_id,
                        principalSchema: "fin",
                        principalTable: "frequency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "gl_account_allocation",
                schema: "anywhereusa",
                columns: table => new
                {
                    category_id = table.Column<string>(type: "text", nullable: false),
                    subcategory_id = table.Column<string>(type: "text", nullable: false),
                    gl_account_number = table.Column<string>(type: "text", nullable: false),
                    percent = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gl_account_allocation", x => new { x.category_id, x.subcategory_id, x.gl_account_number });
                    table.ForeignKey(
                        name: "FK_gl_account_allocation_category_category_id",
                        column: x => x.category_id,
                        principalSchema: "fin",
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_gl_account_allocation_subcategory_subcategory_id",
                        column: x => x.subcategory_id,
                        principalSchema: "fin",
                        principalTable: "subcategory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user",
                schema: "usr",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    username = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    initials = table.Column<string>(type: "text", nullable: true),
                    time_zone_id = table.Column<string>(type: "text", nullable: true),
                    locale_id = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    lowercase_username = table.Column<string>(type: "text", nullable: false, computedColumnSql: "lower(username)", stored: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_locale_locale_id",
                        column: x => x.locale_id,
                        principalSchema: "geo",
                        principalTable: "locale",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_user_tz_time_zone_id",
                        column: x => x.time_zone_id,
                        principalSchema: "geo",
                        principalTable: "tz",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "batch",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_posted = table.Column<bool>(type: "boolean", nullable: false),
                    is_balanced = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    deposit_control_amount = table.Column<decimal>(type: "numeric", nullable: false),
                    total_deposited = table.Column<decimal>(type: "numeric", nullable: true),
                    total_revenue_and_overpayment = table.Column<decimal>(type: "numeric", nullable: true),
                    note = table.Column<string>(type: "text", nullable: true),
                    posted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batch", x => x.id);
                    table.ForeignKey(
                        name: "FK_batch_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "comment",
                schema: "usr",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    html = table.Column<string>(type: "text", nullable: true),
                    text = table.Column<string>(type: "text", nullable: true),
                    author_id = table.Column<int>(type: "integer", nullable: false),
                    posted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_comment_user_author_id",
                        column: x => x.author_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "document",
                schema: "usr",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    posted = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    owner_id = table.Column<int>(type: "integer", nullable: false),
                    name = table.Column<string>(type: "text", nullable: true),
                    content_type = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_document", x => x.id);
                    table.ForeignKey(
                        name: "FK_document_user_owner_id",
                        column: x => x.owner_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "label",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    group_id = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    title = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_label", x => x.id);
                    table.ForeignKey(
                        name: "FK_label_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_label_user_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<string>(type: "character varying(25)", maxLength: 25, nullable: false),
                    status_id = table.Column<string>(type: "text", nullable: true),
                    organization_name = table.Column<string>(type: "text", nullable: false),
                    dba = table.Column<string>(type: "text", nullable: true),
                    organization_description = table.Column<string>(type: "text", nullable: true),
                    classification_id = table.Column<string>(type: "text", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    organization_email = table.Column<string>(type: "text", nullable: true),
                    organization_phone_number = table.Column<string>(type: "text", nullable: true),
                    physical_address_address_lines = table.Column<string>(type: "text", nullable: true),
                    physical_address_city = table.Column<string>(type: "text", nullable: true),
                    physical_address_state_id = table.Column<string>(type: "text", nullable: true),
                    physical_address_zip = table.Column<string>(type: "text", nullable: true),
                    physical_address_full_address = table.Column<string>(type: "text", nullable: true),
                    physical_address_multiline_address = table.Column<string>(type: "text", nullable: true),
                    physical_address_is_empty = table.Column<bool>(type: "boolean", nullable: true),
                    mailing_address_address_lines = table.Column<string>(type: "text", nullable: true),
                    mailing_address_city = table.Column<string>(type: "text", nullable: true),
                    mailing_address_state_id = table.Column<string>(type: "text", nullable: true),
                    mailing_address_zip = table.Column<string>(type: "text", nullable: true),
                    mailing_address_full_address = table.Column<string>(type: "text", nullable: true),
                    mailing_address_multiline_address = table.Column<string>(type: "text", nullable: true),
                    mailing_address_is_empty = table.Column<bool>(type: "boolean", nullable: true),
                    state_id = table.Column<string>(type: "text", nullable: true),
                    federal_id = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_organization_classification_classification_id",
                        column: x => x.classification_id,
                        principalSchema: "org",
                        principalTable: "organization_classification",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_organization_status_status_id",
                        column: x => x.status_id,
                        principalSchema: "org",
                        principalTable: "organization_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_organization_organization_type_type_id",
                        column: x => x.type_id,
                        principalSchema: "org",
                        principalTable: "organization_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_user_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_chart",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<string>(type: "text", nullable: false),
                    frequency_id = table.Column<string>(type: "text", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_chart", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_chart_category_category_id",
                        column: x => x.category_id,
                        principalSchema: "fin",
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_chart_frequency_frequency_id",
                        column: x => x.frequency_id,
                        principalSchema: "fin",
                        principalTable: "frequency",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_chart_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_payment_chart_user_updated_by_id",
                        column: x => x.updated_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "report",
                schema: "ins",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    name = table.Column<string>(type: "text", nullable: false),
                    query_definition_id = table.Column<string>(type: "text", nullable: false),
                    template = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_report", x => x.id);
                    table.ForeignKey(
                        name: "FK_report_query_definition_query_definition_id",
                        column: x => x.query_definition_id,
                        principalSchema: "ins",
                        principalTable: "query_definition",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_report_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_contact_method",
                schema: "usr",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    value = table.Column<string>(type: "text", nullable: false),
                    is_verified = table.Column<bool>(type: "boolean", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
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

            migrationBuilder.CreateTable(
                name: "user_login",
                schema: "usr",
                columns: table => new
                {
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    issuer = table.Column<string>(type: "text", nullable: false),
                    sub = table.Column<string>(type: "text", nullable: false),
                    expires_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_login", x => new { x.user_id, x.issuer, x.sub });
                    table.ForeignKey(
                        name: "FK_user_login_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "transaction",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<int>(type: "integer", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    deposited = table.Column<decimal>(type: "numeric", nullable: true),
                    revenue_and_overpayment = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_batch_batch_id",
                        column: x => x.batch_id,
                        principalSchema: "fin",
                        principalTable: "batch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "batch_comment",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    batch_id = table.Column<int>(type: "integer", nullable: false),
                    comment_id = table.Column<int>(type: "integer", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    created_by_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_batch_comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_batch_comment_batch_batch_id",
                        column: x => x.batch_id,
                        principalSchema: "fin",
                        principalTable: "batch",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_batch_comment_comment_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "usr",
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_batch_comment_user_created_by_id",
                        column: x => x.created_by_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "user_mention",
                schema: "usr",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    user_id = table.Column<int>(type: "integer", nullable: false),
                    comment_id = table.Column<int>(type: "integer", nullable: false),
                    unread = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_user_mention", x => x.id);
                    table.ForeignKey(
                        name: "FK_user_mention_comment_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "usr",
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_user_mention_user_user_id",
                        column: x => x.user_id,
                        principalSchema: "usr",
                        principalTable: "user",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "license",
                schema: "lic",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    title = table.Column<string>(type: "text", nullable: false),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: false),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    issued_date = table.Column<DateTime>(type: "date", nullable: false),
                    expiration_date = table.Column<DateTime>(type: "date", nullable: false),
                    description = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_license", x => x.id);
                    table.ForeignKey(
                        name: "FK_license_license_type_type_id",
                        column: x => x.type_id,
                        principalSchema: "lic",
                        principalTable: "license_type",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_license_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_comment",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: true),
                    comment_id = table.Column<int>(type: "integer", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_comment_comment_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "usr",
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_comment_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "organization_contact",
                schema: "org",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    legal_name = table.Column<string>(type: "text", nullable: false),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: false),
                    relationship_id = table.Column<string>(type: "text", nullable: false),
                    email = table.Column<string>(type: "text", nullable: true),
                    phone_number = table.Column<string>(type: "text", nullable: true),
                    address_address_lines = table.Column<string>(type: "text", nullable: true),
                    address_city = table.Column<string>(type: "text", nullable: true),
                    address_state_id = table.Column<string>(type: "text", nullable: true),
                    address_zip = table.Column<string>(type: "text", nullable: true),
                    address_full_address = table.Column<string>(type: "text", nullable: true),
                    address_multiline_address = table.Column<string>(type: "text", nullable: true),
                    address_is_empty = table.Column<bool>(type: "boolean", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_contact", x => x.id);
                    table.ForeignKey(
                        name: "FK_organization_contact_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_contact_relationship_relationship_id",
                        column: x => x.relationship_id,
                        principalSchema: "org",
                        principalTable: "relationship",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "organization_label",
                schema: "org",
                columns: table => new
                {
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: false),
                    label_id = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_organization_label", x => new { x.organization_id, x.label_id });
                    table.ForeignKey(
                        name: "FK_organization_label_label_label_id",
                        column: x => x.label_id,
                        principalSchema: "org",
                        principalTable: "label",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_organization_label_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "return",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status_id = table.Column<string>(type: "text", nullable: false),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: false),
                    period_id = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_return", x => x.id);
                    table.ForeignKey(
                        name: "FK_return_category_category_id",
                        column: x => x.category_id,
                        principalSchema: "fin",
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_return_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_return_period_period_id",
                        column: x => x.period_id,
                        principalSchema: "fin",
                        principalTable: "period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_return_return_status_status_id",
                        column: x => x.status_id,
                        principalSchema: "fin",
                        principalTable: "return_status",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "statement",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    status_id = table.Column<string>(type: "text", nullable: false),
                    is_assessment = table.Column<bool>(type: "boolean", nullable: false),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: true),
                    notice_date = table.Column<DateTime>(type: "date", nullable: false),
                    assessment_due_date = table.Column<DateTime>(type: "date", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statement", x => x.id);
                    table.ForeignKey(
                        name: "FK_statement_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "filing_schedule",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    payment_chart_id = table.Column<int>(type: "integer", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filing_schedule", x => x.id);
                    table.ForeignKey(
                        name: "FK_filing_schedule_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_filing_schedule_payment_chart_payment_chart_id",
                        column: x => x.payment_chart_id,
                        principalSchema: "fin",
                        principalTable: "payment_chart",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "payment_configuration",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    payment_chart_id = table.Column<int>(type: "integer", nullable: false),
                    start_date = table.Column<DateTime>(type: "date", nullable: false),
                    end_date = table.Column<DateTime>(type: "date", nullable: false),
                    vendor_fee_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    vendor_fee_max = table.Column<decimal>(type: "numeric", nullable: false),
                    penalty_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    interest_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    tax_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    estimated_net_amount_due_percentage = table.Column<decimal>(type: "numeric", nullable: false),
                    minimum_estimated_net_amount_due = table.Column<decimal>(type: "numeric", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_payment_configuration", x => x.id);
                    table.ForeignKey(
                        name: "FK_payment_configuration_payment_chart_payment_chart_id",
                        column: x => x.payment_chart_id,
                        principalSchema: "fin",
                        principalTable: "payment_chart",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "transaction_detail",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    transaction_id = table.Column<int>(type: "integer", nullable: false),
                    bucket_id = table.Column<string>(type: "text", nullable: false),
                    category_id = table.Column<string>(type: "text", nullable: false),
                    subcategory_id = table.Column<string>(type: "text", nullable: false),
                    organization_id = table.Column<string>(type: "character varying(25)", nullable: false),
                    period_id = table.Column<string>(type: "text", nullable: false),
                    effective_date = table.Column<DateTime>(type: "date", nullable: false),
                    amount = table.Column<decimal>(type: "numeric", nullable: false),
                    note = table.Column<string>(type: "text", nullable: true),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_transaction_detail", x => x.id);
                    table.ForeignKey(
                        name: "FK_transaction_detail_bucket_bucket_id",
                        column: x => x.bucket_id,
                        principalSchema: "fin",
                        principalTable: "bucket",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_detail_category_category_id",
                        column: x => x.category_id,
                        principalSchema: "fin",
                        principalTable: "category",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_detail_organization_organization_id",
                        column: x => x.organization_id,
                        principalSchema: "org",
                        principalTable: "organization",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_detail_period_period_id",
                        column: x => x.period_id,
                        principalSchema: "fin",
                        principalTable: "period",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_detail_subcategory_subcategory_id",
                        column: x => x.subcategory_id,
                        principalSchema: "fin",
                        principalTable: "subcategory",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_transaction_detail_transaction_transaction_id",
                        column: x => x.transaction_id,
                        principalSchema: "fin",
                        principalTable: "transaction",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "filing",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    type_id = table.Column<string>(type: "text", nullable: false),
                    return_id = table.Column<int>(type: "integer", nullable: false),
                    filing_date = table.Column<DateTime>(type: "date", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    assessment_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    fee_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    taxable_amount = table.Column<decimal>(type: "numeric", nullable: true),
                    excess_tax = table.Column<decimal>(type: "numeric", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_filing", x => x.id);
                    table.ForeignKey(
                        name: "FK_filing_return_return_id",
                        column: x => x.return_id,
                        principalSchema: "fin",
                        principalTable: "return",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "return_comment",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    return_id = table.Column<int>(type: "integer", nullable: false),
                    comment_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_return_comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_return_comment_comment_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "usr",
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_return_comment_return_return_id",
                        column: x => x.return_id,
                        principalSchema: "fin",
                        principalTable: "return",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "return_label",
                schema: "fin",
                columns: table => new
                {
                    return_id = table.Column<int>(type: "integer", nullable: false),
                    label_id = table.Column<string>(type: "text", nullable: false),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_return_label", x => new { x.return_id, x.label_id });
                    table.ForeignKey(
                        name: "FK_return_label_label_label_id",
                        column: x => x.label_id,
                        principalSchema: "org",
                        principalTable: "label",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_return_label_return_return_id",
                        column: x => x.return_id,
                        principalSchema: "fin",
                        principalTable: "return",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "statement_comment",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    created = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    statement_id = table.Column<int>(type: "integer", nullable: false),
                    comment_id = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_statement_comment", x => x.id);
                    table.ForeignKey(
                        name: "FK_statement_comment_comment_comment_id",
                        column: x => x.comment_id,
                        principalSchema: "usr",
                        principalTable: "comment",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_statement_comment_statement_statement_id",
                        column: x => x.statement_id,
                        principalSchema: "fin",
                        principalTable: "statement",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "statement_detail",
                schema: "fin",
                columns: table => new
                {
                    id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    statement_id = table.Column<int>(type: "integer", nullable: false),
                    return_id = table.Column<int>(type: "integer", nullable: false),
                    tax_due = table.Column<double>(type: "double precision", nullable: false),
                    penalty_due = table.Column<double>(type: "double precision", nullable: false),
                    interest_due = table.Column<double>(type: "double precision", nullable: false),
                    fees_due = table.Column<double>(type: "double precision", nullable: false),
                    nsf_fees_due = table.Column<double>(type: "double precision", nullable: false),
                    total_due = table.Column<double>(type: "double precision", nullable: false),
                    reason_code_id = table.Column<string>(type: "text", nullable: true),
                    bucket_id = table.Column<string>(type: "text", nullable: true),
                    period_id = table.Column<string>(type: "text", nullable: true),
                    subcategory_id = table.Column<string>(type: "text", nullable: true)
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
                name: "IX_batch_created_by_id",
                schema: "fin",
                table: "batch",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_batch_name",
                schema: "fin",
                table: "batch",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_batch_comment_batch_id",
                schema: "fin",
                table: "batch_comment",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_batch_comment_comment_id",
                schema: "fin",
                table: "batch_comment",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_batch_comment_created_by_id",
                schema: "fin",
                table: "batch_comment",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_comment_author_id",
                schema: "usr",
                table: "comment",
                column: "author_id");

            migrationBuilder.CreateIndex(
                name: "IX_document_owner_id",
                schema: "usr",
                table: "document",
                column: "owner_id");

            migrationBuilder.CreateIndex(
                name: "IX_filing_return_id",
                schema: "fin",
                table: "filing",
                column: "return_id");

            migrationBuilder.CreateIndex(
                name: "IX_filing_schedule_organization_id_payment_chart_id_start_date",
                schema: "fin",
                table: "filing_schedule",
                columns: new[] { "organization_id", "payment_chart_id", "start_date" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_filing_schedule_payment_chart_id",
                schema: "fin",
                table: "filing_schedule",
                column: "payment_chart_id");

            migrationBuilder.CreateIndex(
                name: "IX_gl_account_allocation_subcategory_id",
                schema: "anywhereusa",
                table: "gl_account_allocation",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "IX_label_created_by_id",
                schema: "org",
                table: "label",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_label_group_id",
                schema: "org",
                table: "label",
                column: "group_id");

            migrationBuilder.CreateIndex(
                name: "IX_label_title",
                schema: "org",
                table: "label",
                column: "title");

            migrationBuilder.CreateIndex(
                name: "IX_label_updated_by_id",
                schema: "org",
                table: "label",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_license_organization_id",
                schema: "lic",
                table: "license",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_license_type_id_title",
                schema: "lic",
                table: "license",
                columns: new[] { "type_id", "title" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_organization_classification_id",
                schema: "org",
                table: "organization",
                column: "classification_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_created_by_id",
                schema: "org",
                table: "organization",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_status_id",
                schema: "org",
                table: "organization",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_type_id",
                schema: "org",
                table: "organization",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_updated_by_id",
                schema: "org",
                table: "organization",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_comment_comment_id",
                schema: "org",
                table: "organization_comment",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_comment_organization_id",
                schema: "org",
                table: "organization_comment",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_contact_organization_id",
                schema: "org",
                table: "organization_contact",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_contact_relationship_id",
                schema: "org",
                table: "organization_contact",
                column: "relationship_id");

            migrationBuilder.CreateIndex(
                name: "IX_organization_label_label_id",
                schema: "org",
                table: "organization_label",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_chart_category_id",
                schema: "fin",
                table: "payment_chart",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_chart_created_by_id",
                schema: "fin",
                table: "payment_chart",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_chart_frequency_id",
                schema: "fin",
                table: "payment_chart",
                column: "frequency_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_chart_updated_by_id",
                schema: "fin",
                table: "payment_chart",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_payment_configuration_payment_chart_id",
                schema: "fin",
                table: "payment_configuration",
                column: "payment_chart_id");

            migrationBuilder.CreateIndex(
                name: "IX_period_due_date",
                schema: "fin",
                table: "period",
                column: "due_date");

            migrationBuilder.CreateIndex(
                name: "IX_period_end_date",
                schema: "fin",
                table: "period",
                column: "end_date");

            migrationBuilder.CreateIndex(
                name: "IX_period_frequency_id",
                schema: "fin",
                table: "period",
                column: "frequency_id");

            migrationBuilder.CreateIndex(
                name: "IX_period_start_date",
                schema: "fin",
                table: "period",
                column: "start_date");

            migrationBuilder.CreateIndex(
                name: "IX_report_created_by_id",
                schema: "ins",
                table: "report",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_report_query_definition_id",
                schema: "ins",
                table: "report",
                column: "query_definition_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_category_id",
                schema: "fin",
                table: "return",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_organization_id_category_id_period_id",
                schema: "fin",
                table: "return",
                columns: new[] { "organization_id", "category_id", "period_id" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_return_period_id",
                schema: "fin",
                table: "return",
                column: "period_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_status_id",
                schema: "fin",
                table: "return",
                column: "status_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_comment_comment_id",
                schema: "fin",
                table: "return_comment",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_comment_return_id",
                schema: "fin",
                table: "return_comment",
                column: "return_id");

            migrationBuilder.CreateIndex(
                name: "IX_return_label_label_id",
                schema: "fin",
                table: "return_label",
                column: "label_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_organization_id",
                schema: "fin",
                table: "statement",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_comment_comment_id",
                schema: "fin",
                table: "statement_comment",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_statement_comment_statement_id",
                schema: "fin",
                table: "statement_comment",
                column: "statement_id");

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

            migrationBuilder.CreateIndex(
                name: "IX_transaction_batch_id",
                schema: "fin",
                table: "transaction",
                column: "batch_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_bucket_id",
                schema: "fin",
                table: "transaction_detail",
                column: "bucket_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_category_id",
                schema: "fin",
                table: "transaction_detail",
                column: "category_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_organization_id",
                schema: "fin",
                table: "transaction_detail",
                column: "organization_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_period_id",
                schema: "fin",
                table: "transaction_detail",
                column: "period_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_subcategory_id",
                schema: "fin",
                table: "transaction_detail",
                column: "subcategory_id");

            migrationBuilder.CreateIndex(
                name: "IX_transaction_detail_transaction_id",
                schema: "fin",
                table: "transaction_detail",
                column: "transaction_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_locale_id",
                schema: "usr",
                table: "user",
                column: "locale_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_lowercase_username",
                schema: "usr",
                table: "user",
                column: "lowercase_username",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_user_time_zone_id",
                schema: "usr",
                table: "user",
                column: "time_zone_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_contact_method_type_id",
                schema: "usr",
                table: "user_contact_method",
                column: "type_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_mention_comment_id",
                schema: "usr",
                table: "user_mention",
                column: "comment_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_mention_user_id",
                schema: "usr",
                table: "user_mention",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "IX_user_role_role_id",
                schema: "usr",
                table: "user_role",
                column: "role_id");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "batch_comment",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "document",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "filing",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "filing_schedule",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "gl_account_allocation",
                schema: "anywhereusa");

            migrationBuilder.DropTable(
                name: "license",
                schema: "lic");

            migrationBuilder.DropTable(
                name: "organization_comment",
                schema: "org");

            migrationBuilder.DropTable(
                name: "organization_contact",
                schema: "org");

            migrationBuilder.DropTable(
                name: "organization_label",
                schema: "org");

            migrationBuilder.DropTable(
                name: "payment_configuration",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "report",
                schema: "ins");

            migrationBuilder.DropTable(
                name: "return_comment",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "return_label",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "state",
                schema: "geo");

            migrationBuilder.DropTable(
                name: "statement_comment",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "statement_detail",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "transaction_detail",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "user_contact_method",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "user_login",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "user_mention",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "user_role",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "license_type",
                schema: "lic");

            migrationBuilder.DropTable(
                name: "relationship",
                schema: "org");

            migrationBuilder.DropTable(
                name: "payment_chart",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "query_definition",
                schema: "ins");

            migrationBuilder.DropTable(
                name: "label",
                schema: "org");

            migrationBuilder.DropTable(
                name: "return",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "statement_reason_code",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "statement",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "bucket",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "subcategory",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "transaction",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "contact_method_type",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "comment",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "role",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "category",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "period",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "return_status",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "organization",
                schema: "org");

            migrationBuilder.DropTable(
                name: "batch",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "frequency",
                schema: "fin");

            migrationBuilder.DropTable(
                name: "organization_classification",
                schema: "org");

            migrationBuilder.DropTable(
                name: "organization_status",
                schema: "org");

            migrationBuilder.DropTable(
                name: "organization_type",
                schema: "org");

            migrationBuilder.DropTable(
                name: "user",
                schema: "usr");

            migrationBuilder.DropTable(
                name: "locale",
                schema: "geo");

            migrationBuilder.DropTable(
                name: "tz",
                schema: "geo");
        }
    }
}
