using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BusinessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPartnerToPassenger : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "partner_id",
                table: "passengers",
                type: "uuid",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "ix_passengers_partner_id",
                table: "passengers",
                column: "partner_id");

            migrationBuilder.AddForeignKey(
                name: "fk_passengers_partners_partner_id",
                table: "passengers",
                column: "partner_id",
                principalTable: "partners",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_passengers_partners_partner_id",
                table: "passengers");

            migrationBuilder.DropIndex(
                name: "ix_passengers_partner_id",
                table: "passengers");

            migrationBuilder.DropColumn(
                name: "partner_id",
                table: "passengers");

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

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f5555555-5555-5555-5555-555555555555"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4206), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4207) });

            migrationBuilder.UpdateData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f6666666-6666-6666-6666-666666666666"),
                columns: new[] { "created_at", "updated_at" },
                values: new object[] { new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4211), new DateTime(2026, 7, 8, 12, 24, 18, 677, DateTimeKind.Utc).AddTicks(4212) });

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
        }
    }
}
