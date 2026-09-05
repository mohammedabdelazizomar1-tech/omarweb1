using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupNumberToPassenger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "group_number",
                table: "passengers",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(5004), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(5005) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9158), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9160) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9166), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9167) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9172), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9173) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9176), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9177) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9181), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9181) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9185), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9186) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c8888888-8888-8888-8888-888888888888"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9189), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9190) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c9999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9201), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9202) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9206), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9206) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9210), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9211) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9214), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9215) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9364), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(9365) });

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"),
                column: "updated_at",
                value: new DateTime(2026, 7, 12, 15, 24, 7, 178, DateTimeKind.Utc).AddTicks(8468));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"),
                column: "updated_at",
                value: new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(1669));

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 183, DateTimeKind.Utc).AddTicks(6548), new DateTime(2026, 7, 12, 15, 24, 7, 183, DateTimeKind.Utc).AddTicks(6549) });

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 184, DateTimeKind.Utc).AddTicks(1276), new DateTime(2026, 7, 12, 15, 24, 7, 184, DateTimeKind.Utc).AddTicks(1277) });

            migrationBuilder.UpdateData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 185, DateTimeKind.Utc).AddTicks(2467), new DateTime(2026, 7, 12, 15, 24, 7, 185, DateTimeKind.Utc).AddTicks(2468) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(4320), new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(4321) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8133), new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8134) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8156), new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8156) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8161), new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8161) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8241), new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8241) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8245), new DateTime(2026, 7, 12, 15, 24, 7, 179, DateTimeKind.Utc).AddTicks(8246) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 181, DateTimeKind.Utc).AddTicks(4741), new DateTime(2026, 7, 12, 15, 24, 7, 181, DateTimeKind.Utc).AddTicks(4743) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2310), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2311) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2320), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2320) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2326), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2327) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2332), new DateTime(2026, 7, 12, 15, 24, 7, 182, DateTimeKind.Utc).AddTicks(2333) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 184, DateTimeKind.Utc).AddTicks(3231), new DateTime(2026, 7, 12, 15, 24, 7, 184, DateTimeKind.Utc).AddTicks(3233) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 15, 24, 7, 185, DateTimeKind.Utc).AddTicks(978), new DateTime(2026, 7, 12, 15, 24, 7, 185, DateTimeKind.Utc).AddTicks(980) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "group_number",
                table: "passengers");

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(1427), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(1427) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3233), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3233) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3236), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3237) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3239), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3239) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3241), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3241) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3243), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3243) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c7777777-7777-7777-7777-777777777777"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3245), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3246) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c8888888-8888-8888-8888-888888888888"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3247), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3248) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("c9999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3277), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3278) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("caaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3280), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3280) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3282), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3282) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3284), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3284) });

            migrationBuilder.UpdateData(
                table: "chart_of_accounts",
                keyColumn: "id",
                keyValue: new Guid("cddddddd-dddd-dddd-dddd-dddddddddddd"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3286), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(3286) });

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"),
                column: "updated_at",
                value: new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(4700));

            migrationBuilder.UpdateData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"),
                column: "updated_at",
                value: new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(6200));

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(6753), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(6754) });

            migrationBuilder.UpdateData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(8964), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(8964) });

            migrationBuilder.UpdateData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 897, DateTimeKind.Utc).AddTicks(4099), new DateTime(2026, 7, 12, 12, 19, 53, 897, DateTimeKind.Utc).AddTicks(4100) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(7444), new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(7445) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9249), new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9250) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9262), new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9262) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9264), new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9264) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9266), new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9267) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9269), new DateTime(2026, 7, 12, 12, 19, 53, 894, DateTimeKind.Utc).AddTicks(9269) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 895, DateTimeKind.Utc).AddTicks(7103), new DateTime(2026, 7, 12, 12, 19, 53, 895, DateTimeKind.Utc).AddTicks(7103) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 895, DateTimeKind.Utc).AddTicks(9990), new DateTime(2026, 7, 12, 12, 19, 53, 895, DateTimeKind.Utc).AddTicks(9991) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 895, DateTimeKind.Utc).AddTicks(9995), new DateTime(2026, 7, 12, 12, 19, 53, 895, DateTimeKind.Utc).AddTicks(9995) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(26), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(27) });

            migrationBuilder.UpdateData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(30), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(30) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(9941), new DateTime(2026, 7, 12, 12, 19, 53, 896, DateTimeKind.Utc).AddTicks(9942) });

            migrationBuilder.UpdateData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 12, 12, 19, 53, 897, DateTimeKind.Utc).AddTicks(3375), new DateTime(2026, 7, 12, 12, 19, 53, 897, DateTimeKind.Utc).AddTicks(3375) });
        }
    }
}
