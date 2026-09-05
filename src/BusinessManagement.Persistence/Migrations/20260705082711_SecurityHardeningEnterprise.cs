using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusinessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecurityHardeningEnterprise : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_passengers_passport_number_nationality",
                table: "passengers");

            migrationBuilder.AddColumn<int>(
                name: "access_failed_count",
                table: "users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "email_confirmed",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "email_verification_token",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "lockout_end",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "password_reset_expiry",
                table: "users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "password_reset_token",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "profile_picture_url",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "two_factor_enabled",
                table: "users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "two_factor_secret",
                table: "users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "national_id_hash",
                table: "passengers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "passport_number_hash",
                table: "passengers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "group_number",
                table: "bookings",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "log_hash",
                table: "audit_logs",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "previous_log_hash",
                table: "audit_logs",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "monthly_closings",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    year = table.Column<int>(type: "integer", nullable: false),
                    month = table.Column<int>(type: "integer", nullable: false),
                    is_closed = table.Column<bool>(type: "boolean", nullable: false),
                    closed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    closed_by = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_monthly_closings", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "passenger_notes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    passenger_id = table.Column<Guid>(type: "uuid", nullable: false),
                    note_text = table.Column<string>(type: "text", nullable: false),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_passenger_notes", x => x.id);
                    table.ForeignKey(
                        name: "fk_passenger_notes_passengers_passenger_id",
                        column: x => x.passenger_id,
                        principalTable: "passengers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_mfa_recovery_codes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    code_hash = table.Column<string>(type: "text", nullable: false),
                    consumed_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    xmin = table.Column<uint>(type: "xid", rowVersion: true, nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    created_by = table.Column<string>(type: "text", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_by = table.Column<string>(type: "text", nullable: false),
                    is_deleted = table.Column<bool>(type: "boolean", nullable: false),
                    deleted_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    deleted_by = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_mfa_recovery_codes", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_mfa_recovery_codes_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "user_password_histories",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    user_id = table.Column<Guid>(type: "uuid", nullable: false),
                    password_hash = table.Column<string>(type: "text", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_user_password_histories", x => x.id);
                    table.ForeignKey(
                        name: "fk_user_password_histories_users_user_id",
                        column: x => x.user_id,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "chart_of_accounts",
                columns: new[] { "id", "account_code", "created_at", "created_by", "deleted_at", "deleted_by", "is_active", "is_deleted", "name", "tenant_id", "type", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("c1111111-1111-1111-1111-111111111111"), "1101", new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(8897), "system", null, null, true, false, "صندوق النقدية (ج.م)", new Guid("e1111111-1111-1111-1111-111111111111"), "Asset", new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(8899), "system" },
                    { new Guid("c2222222-2222-2222-2222-222222222222"), "1102", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7345), "system", null, null, true, false, "صندوق النقدية (ر.س)", new Guid("e1111111-1111-1111-1111-111111111111"), "Asset", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7348), "system" },
                    { new Guid("c3333333-3333-3333-3333-333333333333"), "1201", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7357), "system", null, null, true, false, "حسابات ذمم العملاء المدينة", new Guid("e1111111-1111-1111-1111-111111111111"), "Asset", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7358), "system" },
                    { new Guid("c4444444-4444-4444-4444-444444444444"), "2101", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7376), "system", null, null, true, false, "حسابات دائنة للموردين والشركاء", new Guid("e1111111-1111-1111-1111-111111111111"), "Liability", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7377), "system" },
                    { new Guid("c5555555-5555-5555-5555-555555555555"), "3101", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7383), "system", null, null, true, false, "رأس مال الشركاء", new Guid("e1111111-1111-1111-1111-111111111111"), "Equity", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7384), "system" },
                    { new Guid("c6666666-6666-6666-6666-666666666666"), "3201", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7390), "system", null, null, true, false, "الأرباح المحتجزة / المجمعة", new Guid("e1111111-1111-1111-1111-111111111111"), "Equity", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7390), "system" },
                    { new Guid("c7777777-7777-7777-7777-777777777777"), "4101", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7396), "system", null, null, true, false, "إيرادات مبيعات الحجوزات", new Guid("e1111111-1111-1111-1111-111111111111"), "Revenue", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7396), "system" },
                    { new Guid("c8888888-8888-8888-8888-888888888888"), "5101", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7401), "system", null, null, true, false, "مصروفات تكلفة الفيزا والمنصة", new Guid("e1111111-1111-1111-1111-111111111111"), "Expense", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7402), "system" },
                    { new Guid("c9999999-9999-9999-9999-999999999999"), "5102", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7407), "system", null, null, true, false, "مصروفات تذاكر الطيران", new Guid("e1111111-1111-1111-1111-111111111111"), "Expense", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7407), "system" },
                    { new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"), "5103", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7413), "system", null, null, true, false, "مصروفات فنادق وتسكين البرامج", new Guid("e1111111-1111-1111-1111-111111111111"), "Expense", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7413), "system" },
                    { new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"), "5104", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7436), "system", null, null, true, false, "مصروفات انتقالات وباصات", new Guid("e1111111-1111-1111-1111-111111111111"), "Expense", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7437), "system" },
                    { new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"), "5105", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7449), "system", null, null, true, false, "مصروفات باركود وبوابات", new Guid("e1111111-1111-1111-1111-111111111111"), "Expense", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7450), "system" },
                    { new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"), "5201", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7455), "system", null, null, true, false, "مصروفات إدارية وعامة", new Guid("e1111111-1111-1111-1111-111111111111"), "Expense", new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7456), "system" }
                });

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"),
                column: "updated_at",
                value: new DateTime(2026, 7, 5, 8, 27, 9, 12, DateTimeKind.Utc).AddTicks(369));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"),
                column: "updated_at",
                value: new DateTime(2026, 7, 5, 8, 27, 9, 12, DateTimeKind.Utc).AddTicks(6120));

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 19, DateTimeKind.Utc).AddTicks(8649), new DateTime(2026, 7, 5, 8, 27, 9, 19, DateTimeKind.Utc).AddTicks(8651) });

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 20, DateTimeKind.Utc).AddTicks(6487), new DateTime(2026, 7, 5, 8, 27, 9, 20, DateTimeKind.Utc).AddTicks(6488) });

            migrationBuilder.UpdateData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 22, DateTimeKind.Utc).AddTicks(4214), new DateTime(2026, 7, 5, 8, 27, 9, 22, DateTimeKind.Utc).AddTicks(4216) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 12, DateTimeKind.Utc).AddTicks(9874), new DateTime(2026, 7, 5, 8, 27, 9, 12, DateTimeKind.Utc).AddTicks(9875) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 13, DateTimeKind.Utc).AddTicks(6026), new DateTime(2026, 7, 5, 8, 27, 9, 13, DateTimeKind.Utc).AddTicks(6028) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 13, DateTimeKind.Utc).AddTicks(6038), new DateTime(2026, 7, 5, 8, 27, 9, 13, DateTimeKind.Utc).AddTicks(6039) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 13, DateTimeKind.Utc).AddTicks(6222), new DateTime(2026, 7, 5, 8, 27, 9, 13, DateTimeKind.Utc).AddTicks(6222) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 16, DateTimeKind.Utc).AddTicks(2230), new DateTime(2026, 7, 5, 8, 27, 9, 16, DateTimeKind.Utc).AddTicks(2232) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5047), new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5050) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5062), new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5063) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5084), new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5085) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5092), new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(5093) });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "id", "code", "description" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-111111111111"), "bookings.view", "عرض دفتر الحجوزات" },
                    { new Guid("e2222222-2222-2222-2222-222222222222"), "bookings.create", "تسجيل حجز جديد" },
                    { new Guid("e3333333-3333-3333-3333-333333333333"), "bookings.edit", "تعديل تفاصيل الحجز" },
                    { new Guid("e4444444-4444-4444-4444-444444444444"), "bookings.delete", "حذف وأرشفة الحجوزات" },
                    { new Guid("e5555555-5555-5555-5555-555555555555"), "safe.view", "عرض وإدارة الخزينة والشركاء" },
                    { new Guid("e6666666-6666-6666-6666-666666666666"), "roles.manage", "إدارة الأدوار وصلاحيات النظام" }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "name" },
                values: new object[,]
                {
                    { new Guid("f1111111-1111-1111-1111-111111111111"), "Admin" },
                    { new Guid("f2222222-2222-2222-2222-222222222222"), "Manager" },
                    { new Guid("f3333333-3333-3333-3333-333333333333"), "Employee" },
                    { new Guid("f4444444-4444-4444-4444-444444444444"), "Partner" }
                });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 20, DateTimeKind.Utc).AddTicks(9836), new DateTime(2026, 7, 5, 8, 27, 9, 20, DateTimeKind.Utc).AddTicks(9838) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 22, DateTimeKind.Utc).AddTicks(1965), new DateTime(2026, 7, 5, 8, 27, 9, 22, DateTimeKind.Utc).AddTicks(1967) });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"),
                columns: new[] { "access_failed_count", "email_confirmed", "email_verification_token", "lockout_end", "password_reset_expiry", "password_reset_token", "profile_picture_url", "two_factor_enabled", "two_factor_secret" },
                values: new object[] { 0, false, null, null, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"),
                columns: new[] { "access_failed_count", "email_confirmed", "email_verification_token", "lockout_end", "password_reset_expiry", "password_reset_token", "profile_picture_url", "two_factor_enabled", "two_factor_secret" },
                values: new object[] { 0, false, null, null, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"),
                columns: new[] { "access_failed_count", "email_confirmed", "email_verification_token", "lockout_end", "password_reset_expiry", "password_reset_token", "profile_picture_url", "two_factor_enabled", "two_factor_secret" },
                values: new object[] { 0, false, null, null, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"),
                columns: new[] { "access_failed_count", "email_confirmed", "email_verification_token", "lockout_end", "password_reset_expiry", "password_reset_token", "profile_picture_url", "two_factor_enabled", "two_factor_secret" },
                values: new object[] { 0, false, null, null, null, null, null, false, null });

            migrationBuilder.UpdateData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a5555555-5555-5555-5555-555555555555"),
                columns: new[] { "access_failed_count", "email_confirmed", "email_verification_token", "lockout_end", "password_reset_expiry", "password_reset_token", "profile_picture_url", "two_factor_enabled", "two_factor_secret" },
                values: new object[] { 0, false, null, null, null, null, null, false, null });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "permission_id", "role_id" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-111111111111"), new Guid("f1111111-1111-1111-1111-111111111111") },
                    { new Guid("e2222222-2222-2222-2222-222222222222"), new Guid("f1111111-1111-1111-1111-111111111111") },
                    { new Guid("e3333333-3333-3333-3333-333333333333"), new Guid("f1111111-1111-1111-1111-111111111111") },
                    { new Guid("e4444444-4444-4444-4444-444444444444"), new Guid("f1111111-1111-1111-1111-111111111111") },
                    { new Guid("e5555555-5555-5555-5555-555555555555"), new Guid("f1111111-1111-1111-1111-111111111111") },
                    { new Guid("e6666666-6666-6666-6666-666666666666"), new Guid("f1111111-1111-1111-1111-111111111111") }
                });

            migrationBuilder.InsertData(
                table: "user_roles",
                columns: new[] { "role_id", "user_id" },
                values: new object[] { new Guid("f1111111-1111-1111-1111-111111111111"), new Guid("a1111111-1111-1111-1111-111111111111") });

            migrationBuilder.CreateIndex(
                name: "IX_passengers_passport_number_hash_nationality",
                table: "passengers",
                columns: new[] { "passport_number_hash", "nationality" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_passenger_notes_passenger_id",
                table: "passenger_notes",
                column: "passenger_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_mfa_recovery_codes_user_id",
                table: "user_mfa_recovery_codes",
                column: "user_id");

            migrationBuilder.CreateIndex(
                name: "ix_user_password_histories_user_id",
                table: "user_password_histories",
                column: "user_id");

            // Enforce True WORM Log Immutability at database-level via PostgreSQL BEFORE triggers
            migrationBuilder.Sql(@"
                CREATE OR REPLACE FUNCTION prevent_audit_modification()
                RETURNS TRIGGER LANGUAGE plpgsql AS $$
                BEGIN
                    RAISE EXCEPTION 'Audit/Activity logs are strictly immutable and cannot be updated or deleted.';
                END;
                $$;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER audit_logs_prevent_modification
                BEFORE UPDATE OR DELETE ON audit_logs
                FOR EACH ROW EXECUTE FUNCTION prevent_audit_modification();
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER activity_logs_prevent_modification
                BEFORE UPDATE OR DELETE ON activity_logs
                FOR EACH ROW EXECUTE FUNCTION prevent_audit_modification();
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS audit_logs_prevent_modification ON audit_logs;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS activity_logs_prevent_modification ON activity_logs;");
            migrationBuilder.Sql("DROP FUNCTION IF EXISTS prevent_audit_modification();");

            migrationBuilder.DropTable(
                name: "monthly_closings");

            migrationBuilder.DropTable(
                name: "passenger_notes");

            migrationBuilder.DropTable(
                name: "user_mfa_recovery_codes");

            migrationBuilder.DropTable(
                name: "user_password_histories");

            migrationBuilder.DropIndex(
                name: "IX_passengers_passport_number_hash_nationality",
                table: "passengers");

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c8888888-8888-8888-8888-888888888888"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c9999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"));

            migrationBuilder.DeleteData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"));

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e1111111-1111-1111-1111-111111111111"), new Guid("f1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e2222222-2222-2222-2222-222222222222"), new Guid("f1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e3333333-3333-3333-3333-333333333333"), new Guid("f1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e4444444-4444-4444-4444-444444444444"), new Guid("f1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e5555555-5555-5555-5555-555555555555"), new Guid("f1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumns: new[] { "permission_id", "role_id" },
                keyValues: new object[] { new Guid("e6666666-6666-6666-6666-666666666666"), new Guid("f1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "user_roles",
                keyColumns: new[] { "role_id", "user_id" },
                keyValues: new object[] { new Guid("f1111111-1111-1111-1111-111111111111"), new Guid("a1111111-1111-1111-1111-111111111111") });

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "id",
                keyValue: new Guid("e6666666-6666-6666-6666-666666666666"));

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DropColumn(
                name: "access_failed_count",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email_confirmed",
                table: "users");

            migrationBuilder.DropColumn(
                name: "email_verification_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "lockout_end",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_expiry",
                table: "users");

            migrationBuilder.DropColumn(
                name: "password_reset_token",
                table: "users");

            migrationBuilder.DropColumn(
                name: "profile_picture_url",
                table: "users");

            migrationBuilder.DropColumn(
                name: "two_factor_enabled",
                table: "users");

            migrationBuilder.DropColumn(
                name: "two_factor_secret",
                table: "users");

            migrationBuilder.DropColumn(
                name: "national_id_hash",
                table: "passengers");

            migrationBuilder.DropColumn(
                name: "passport_number_hash",
                table: "passengers");

            migrationBuilder.DropColumn(
                name: "group_number",
                table: "bookings");

            migrationBuilder.DropColumn(
                name: "log_hash",
                table: "audit_logs");

            migrationBuilder.DropColumn(
                name: "previous_log_hash",
                table: "audit_logs");

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"),
                column: "updated_at",
                value: new DateTime(2026, 7, 3, 16, 39, 36, 137, DateTimeKind.Utc).AddTicks(5612));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"),
                column: "updated_at",
                value: new DateTime(2026, 7, 3, 16, 39, 36, 137, DateTimeKind.Utc).AddTicks(9296));

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(2496), new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(2498) });

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(7683), new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(7685) });

            migrationBuilder.UpdateData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(9270), new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(9271) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(3222), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(3225) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8251), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8253) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8260), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8261) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8266), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8266) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 140, DateTimeKind.Utc).AddTicks(6740), new DateTime(2026, 7, 3, 16, 39, 36, 140, DateTimeKind.Utc).AddTicks(6743) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4409), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4412) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4422), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4423) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4429), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4430) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4436), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4436) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(142), new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(144) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(7732), new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(7735) });

            migrationBuilder.CreateIndex(
                name: "IX_passengers_passport_number_nationality",
                table: "passengers",
                columns: new[] { "passport_number", "nationality" },
                unique: true);
        }
    }
}
