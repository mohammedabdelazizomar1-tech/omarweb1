using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OptimizeAuditLogIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
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

            migrationBuilder.CreateIndex(
                name: "IX_audit_logs_created_at",
                table: "audit_logs",
                column: "created_at");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_audit_logs_created_at",
                table: "audit_logs");

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(8897), new DateTime(2026, 7, 5, 8, 27, 9, 17, DateTimeKind.Utc).AddTicks(8899) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7345), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7348) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7357), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7358) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7376), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7377) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7383), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7384) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7390), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7390) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7396), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7396) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c8888888-8888-8888-8888-888888888888"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7401), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7402) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c9999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7407), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7407) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7413), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7413) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7436), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7437) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7449), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7450) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7455), new DateTime(2026, 7, 5, 8, 27, 9, 18, DateTimeKind.Utc).AddTicks(7456) });

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
        }
    }
}
