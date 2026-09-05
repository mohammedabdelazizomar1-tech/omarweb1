using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusinessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateOutboxMessageForEnterpriseReplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "is_processed",
                table: "outbox_messages");

            migrationBuilder.AddColumn<Guid>(
                name: "aggregate_id",
                table: "outbox_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "aggregate_type",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "correlation_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "device_id",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "event_id",
                table: "outbox_messages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<int>(
                name: "event_version",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "last_error",
                table: "outbox_messages",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "next_retry_at",
                table: "outbox_messages",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "retry_count",
                table: "outbox_messages",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<long>(
                name: "sequence_id",
                table: "outbox_messages",
                type: "bigint",
                nullable: false,
                defaultValue: 0L)
                .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn);

            migrationBuilder.AddColumn<string>(
                name: "status",
                table: "outbox_messages",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "processed_events",
                columns: table => new
                {
                    event_id = table.Column<Guid>(type: "uuid", nullable: false),
                    processed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    device_id = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_processed_events", x => x.event_id);
                });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(5286), new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(5287) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(768), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(770) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(776), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(777) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(782), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(783) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(786), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(787) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(790), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(791) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(805), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(806) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c8888888-8888-8888-8888-888888888888"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(809), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(810) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c9999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(813), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(814) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(817), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(818) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(822), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(823) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(826), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(826) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(882), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(883) });

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"),
                column: "updated_at",
                value: new DateTime(2026, 7, 8, 12, 24, 18, 676, DateTimeKind.Utc).AddTicks(1129));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"),
                column: "updated_at",
                value: new DateTime(2026, 7, 8, 12, 24, 18, 676, DateTimeKind.Utc).AddTicks(4510));

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(9294), new DateTime(2026, 7, 8, 12, 24, 18, 681, DateTimeKind.Utc).AddTicks(9295) });

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 682, DateTimeKind.Utc).AddTicks(4884), new DateTime(2026, 7, 8, 12, 24, 18, 682, DateTimeKind.Utc).AddTicks(4885) });

            migrationBuilder.UpdateData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 683, DateTimeKind.Utc).AddTicks(5958), new DateTime(2026, 7, 8, 12, 24, 18, 683, DateTimeKind.Utc).AddTicks(5960) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 676, DateTimeKind.Utc).AddTicks(7319), new DateTime(2026, 7, 8, 12, 24, 18, 676, DateTimeKind.Utc).AddTicks(7320) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4187), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4188) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4197), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4198) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4202), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4203) });

            migrationBuilder.InsertData(
                table: "partners",
                columns: new[] { "id", "address", "commercial_register", "created_at", "created_by", "deleted_at", "deleted_by", "email", "is_active", "is_deleted", "name", "phone", "quota_limit", "tax_number", "tenant_id", "updated_at", "updated_by", "username" },
                values: new object[,]
                {
                    { new Guid("f5555555-5555-5555-5555-555555555555"), "", "", new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4206), "system", null, null, "", true, false, "Waheed", "", 60, "", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4207), "system", "waheed" },
                    { new Guid("f6666666-6666-6666-6666-666666666666"), "", "", new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4211), "system", null, null, "", true, false, "Kelany", "", 60, "", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4212), "system", "kelany" }
                });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 679, DateTimeKind.Utc).AddTicks(5710), new DateTime(2026, 7, 8, 12, 24, 18, 679, DateTimeKind.Utc).AddTicks(5712) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2156), new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2157) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2166), new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2167) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2183), new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2184) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2248), new DateTime(2026, 7, 8, 12, 24, 18, 680, DateTimeKind.Utc).AddTicks(2249) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 682, DateTimeKind.Utc).AddTicks(6992), new DateTime(2026, 7, 8, 12, 24, 18, 682, DateTimeKind.Utc).AddTicks(6993) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 683, DateTimeKind.Utc).AddTicks(4472), new DateTime(2026, 7, 8, 12, 24, 18, 683, DateTimeKind.Utc).AddTicks(4473) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "email_confirmed",
                value: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "email_confirmed",
                value: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "email_confirmed",
                value: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"),
                column: "email_confirmed",
                value: true);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a5555555-5555-5555-5555-555555555555"),
                column: "email_confirmed",
                value: true);

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "access_failed_count", "branch_id", "created_at", "created_by", "deleted_at", "deleted_by", "department_id", "email", "email_confirmed", "email_verification_token", "first_name", "is_active", "is_deleted", "last_name", "lockout_end", "password_hash", "password_reset_expiry", "password_reset_token", "profile_picture_url", "role", "tenant_id", "two_factor_enabled", "two_factor_secret", "updated_at", "updated_by", "username" },
                values: new object[,]
                {
                    { new Guid("a6666666-6666-6666-6666-666666666666"), 0, new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "waheed@BusinessManagement.com", true, null, "Waheed", true, false, "Shareholder", null, "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=", null, null, null, "Partner", new Guid("e1111111-1111-1111-1111-111111111111"), false, null, new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "waheed" },
                    { new Guid("a7777777-7777-7777-7777-777777777777"), 0, new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "kelany@BusinessManagement.com", true, null, "Kelany", true, false, "Shareholder", null, "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=", null, null, null, "Partner", new Guid("e1111111-1111-1111-1111-111111111111"), false, null, new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "kelany" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_outbox_messages_status_next_retry_at",
                table: "outbox_messages",
                columns: new[] { "status", "next_retry_at" });

            migrationBuilder.CreateIndex(
                name: "IX_processed_events_event_id",
                table: "processed_events",
                column: "event_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "processed_events");

            migrationBuilder.DropIndex(
                name: "IX_outbox_messages_status_next_retry_at",
                table: "outbox_messages");

            migrationBuilder.DeleteData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a7777777-7777-7777-7777-777777777777"));

            migrationBuilder.DropColumn(
                name: "aggregate_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "aggregate_type",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "correlation_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "device_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "event_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "event_version",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "last_error",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "next_retry_at",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "retry_count",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "sequence_id",
                table: "outbox_messages");

            migrationBuilder.DropColumn(
                name: "status",
                table: "outbox_messages");

            migrationBuilder.AddColumn<bool>(
                name: "is_processed",
                table: "outbox_messages",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(9840), new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(9843) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5266), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5267) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5276), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5276) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5281), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5281) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5285), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5286) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5300), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5300) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5304), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5305) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c8888888-8888-8888-8888-888888888888"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5309), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5310) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c9999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5313), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5314) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5386), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5387) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5404), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5405) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5409), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5410) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5413), new DateTime(2026, 7, 5, 8, 46, 56, 416, DateTimeKind.Utc).AddTicks(5414) });

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"),
                column: "updated_at",
                value: new DateTime(2026, 7, 5, 8, 46, 56, 409, DateTimeKind.Utc).AddTicks(4604));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"),
                column: "updated_at",
                value: new DateTime(2026, 7, 5, 8, 46, 56, 409, DateTimeKind.Utc).AddTicks(9599));

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 417, DateTimeKind.Utc).AddTicks(7458), new DateTime(2026, 7, 5, 8, 46, 56, 417, DateTimeKind.Utc).AddTicks(7461) });

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 418, DateTimeKind.Utc).AddTicks(6040), new DateTime(2026, 7, 5, 8, 46, 56, 418, DateTimeKind.Utc).AddTicks(6044) });

            migrationBuilder.UpdateData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 420, DateTimeKind.Utc).AddTicks(2671), new DateTime(2026, 7, 5, 8, 46, 56, 420, DateTimeKind.Utc).AddTicks(2673) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 410, DateTimeKind.Utc).AddTicks(5406), new DateTime(2026, 7, 5, 8, 46, 56, 410, DateTimeKind.Utc).AddTicks(5408) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 411, DateTimeKind.Utc).AddTicks(3999), new DateTime(2026, 7, 5, 8, 46, 56, 411, DateTimeKind.Utc).AddTicks(4003) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 411, DateTimeKind.Utc).AddTicks(4037), new DateTime(2026, 7, 5, 8, 46, 56, 411, DateTimeKind.Utc).AddTicks(4037) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 411, DateTimeKind.Utc).AddTicks(4085), new DateTime(2026, 7, 5, 8, 46, 56, 411, DateTimeKind.Utc).AddTicks(4086) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 414, DateTimeKind.Utc).AddTicks(3862), new DateTime(2026, 7, 5, 8, 46, 56, 414, DateTimeKind.Utc).AddTicks(3865) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6188), new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6192) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6220), new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6220) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6350), new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6351) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6362), new DateTime(2026, 7, 5, 8, 46, 56, 415, DateTimeKind.Utc).AddTicks(6363) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 418, DateTimeKind.Utc).AddTicks(8888), new DateTime(2026, 7, 5, 8, 46, 56, 418, DateTimeKind.Utc).AddTicks(8891) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 46, 56, 419, DateTimeKind.Utc).AddTicks(9779), new DateTime(2026, 7, 5, 8, 46, 56, 419, DateTimeKind.Utc).AddTicks(9788) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                column: "email_confirmed",
                value: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                column: "email_confirmed",
                value: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                column: "email_confirmed",
                value: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"),
                column: "email_confirmed",
                value: false);

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a5555555-5555-5555-5555-555555555555"),
                column: "email_confirmed",
                value: false);
        }
    }
}
