using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BusinessManagement.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSeedPasswords : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "tenants",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    name = table.Column<string>(type: "text", nullable: false),
                    subdomain = table.Column<string>(type: "text", nullable: false),
                    is_active = table.Column<bool>(type: "boolean", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_tenants", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "companies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    tenant_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    commercial_register = table.Column<string>(type: "text", nullable: false),
                    tax_number = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_companies", x => x.id);
                    table.ForeignKey(
                        name: "fk_companies_tenants_tenant_id",
                        column: x => x.tenant_id,
                        principalTable: "tenants",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "branches",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    company_id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("pk_branches", x => x.id);
                    table.ForeignKey(
                        name: "fk_branches_companies_company_id",
                        column: x => x.company_id,
                        principalTable: "companies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "currencies",
                columns: new[] { "id", "code", "is_active", "name", "symbol" },
                values: new object[,]
                {
                    { new Guid("e1111111-1111-1111-1111-111111111111"), "EGP", true, "Egyptian Pound", "ج.م" },
                    { new Guid("e2222222-2222-2222-2222-222222222222"), "SAR", true, "Saudi Riyal", "ر.س" }
                });

            migrationBuilder.InsertData(
                table: "departments",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "description", "is_deleted", "name", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("d1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Agency Executive Directors", false, "Management", new DateTime(2026, 7, 3, 16, 39, 36, 137, DateTimeKind.Utc).AddTicks(5612), "system" },
                    { new Guid("d2222222-2222-2222-2222-222222222222"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, "Umrah and Visas Desk staff", false, "Operations", new DateTime(2026, 7, 3, 16, 39, 36, 137, DateTimeKind.Utc).AddTicks(9296), "system" }
                });

            migrationBuilder.InsertData(
                table: "gateway_portals",
                columns: new[] { "id", "amount", "count", "created_at", "created_by", "deleted_at", "deleted_by", "is_deleted", "notes", "partner_id", "service_type", "tenant_id", "transaction_date", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("41111111-1111-1111-1111-111111111111"), 150000.0m, 1000, new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(2496), "system", null, null, false, "Saudi Gateway initial barcodes package", null, "QrCode", new Guid("e1111111-1111-1111-1111-111111111111"), new DateOnly(2026, 7, 1), new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(2498), "system" },
                    { new Guid("42222222-2222-2222-2222-222222222222"), 95000.0m, 500, new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(7683), "system", null, null, false, "Saudi Portal initial VIP package", null, "Vip", new Guid("e1111111-1111-1111-1111-111111111111"), new DateOnly(2026, 7, 1), new DateTime(2026, 7, 3, 16, 39, 36, 142, DateTimeKind.Utc).AddTicks(7685), "system" }
                });

            migrationBuilder.InsertData(
                table: "packages",
                columns: new[] { "id", "created_at", "created_by", "deleted_at", "deleted_by", "is_deleted", "name", "nights_count", "tenant_id", "total_base_price", "updated_at", "updated_by" },
                values: new object[] { new Guid("99999999-9999-9999-9999-999999999999"), new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(9270), "system", null, null, false, "Default Package", 0, new Guid("e1111111-1111-1111-1111-111111111111"), 0.0m, new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(9271), "system" });

            migrationBuilder.InsertData(
                table: "partners",
                columns: new[] { "id", "address", "commercial_register", "created_at", "created_by", "deleted_at", "deleted_by", "email", "is_active", "is_deleted", "name", "phone", "quota_limit", "tax_number", "tenant_id", "updated_at", "updated_by", "username" },
                values: new object[,]
                {
                    { new Guid("f1111111-1111-1111-1111-111111111111"), "", "", new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(3222), "system", null, null, "", true, false, "Khaled", "", 100, "", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(3225), "system", "khaled" },
                    { new Guid("f2222222-2222-2222-2222-222222222222"), "", "", new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8251), "system", null, null, "", true, false, "Alaa", "", 80, "", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8253), "system", "alaa" },
                    { new Guid("f3333333-3333-3333-3333-333333333333"), "", "", new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8260), "system", null, null, "", true, false, "Omar", "", 90, "", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8261), "system", "omar" },
                    { new Guid("f4444444-4444-4444-4444-444444444444"), "", "", new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8266), "system", null, null, "", true, false, "Mohamed", "", 50, "", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 138, DateTimeKind.Utc).AddTicks(8266), "system", "mohamed" }
                });

            migrationBuilder.InsertData(
                table: "partnership_capitals",
                columns: new[] { "id", "amount_egp", "amount_sar", "created_at", "created_by", "deleted_at", "deleted_by", "historical_rate", "is_deleted", "notes", "profit_share_ratio", "share_ratio", "shareholder_name", "tenant_id", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("31111111-1111-1111-1111-111111111111"), 329810.0m, 25370.0m, new DateTime(2026, 7, 3, 16, 39, 36, 140, DateTimeKind.Utc).AddTicks(6740), "system", null, null, 13.0m, false, "Capital deposit in EGP bank and Riyals", 0.2125m, 0.219871m, "علاء", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 140, DateTimeKind.Utc).AddTicks(6743), "system" },
                    { new Guid("32222222-2222-2222-2222-222222222222"), 260000.0m, 20000.0m, new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4409), "system", null, null, 13.0m, false, "Capital deposit", 0.2125m, 0.173332m, "خالد", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4412), "system" },
                    { new Guid("33333333-3333-3333-3333-333333333333"), 450000.0m, 34600.0m, new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4422), "system", null, null, 13.0m, false, "Capital deposit", 0.2125m, 0.299998m, "عمر", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4423), "system" },
                    { new Guid("34444444-4444-4444-4444-444444444444"), 299000.0m, 23000.0m, new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4429), "system", null, null, 13.0m, false, "Star Shine capital", 0.1500m, 0.199332m, "وحيد", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4430), "system" },
                    { new Guid("35555555-5555-5555-5555-555555555555"), 161200.0m, 12400.0m, new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4436), "system", null, null, 13.0m, false, "Quota shareholder", 0.2125m, 0.107465m, "كيلاني", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(2026, 7, 3, 16, 39, 36, 141, DateTimeKind.Utc).AddTicks(4436), "system" }
                });

            migrationBuilder.InsertData(
                table: "safe_transactions",
                columns: new[] { "id", "amount", "associated_partner_id", "bank_name", "created_at", "created_by", "currency", "deleted_at", "deleted_by", "depositor_or_withdrawer_name", "description", "exchange_rate", "is_deleted", "tenant_id", "transaction_date", "transaction_type", "updated_at", "updated_by" },
                values: new object[,]
                {
                    { new Guid("51111111-1111-1111-1111-111111111111"), 1500010.0m, null, "Safe Cash box", new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(142), "system", "EGP", null, null, "Shareholders", "Aggregated EGP Capital Contribution Pool", 1.0m, false, new Guid("e1111111-1111-1111-1111-111111111111"), new DateOnly(2026, 7, 1), "Deposit", new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(144), "system" },
                    { new Guid("52222222-2222-2222-2222-222222222222"), 115370.0m, null, "Safe Cash box", new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(7732), "system", "SAR", null, null, "Shareholders", "Aggregated Riyals Capital Pool", 13.0m, false, new Guid("e1111111-1111-1111-1111-111111111111"), new DateOnly(2026, 7, 1), "Deposit", new DateTime(2026, 7, 3, 16, 39, 36, 143, DateTimeKind.Utc).AddTicks(7735), "system" }
                });

            migrationBuilder.InsertData(
                table: "tenants",
                columns: new[] { "id", "created_at", "is_active", "name", "subdomain" },
                values: new object[] { new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), true, "Enterprise Tenant", "agency" });

            migrationBuilder.InsertData(
                table: "companies",
                columns: new[] { "id", "commercial_register", "name", "tax_number", "tenant_id" },
                values: new object[] { new Guid("c1111111-1111-1111-1111-111111111111"), "10101010", "Hajj & Umrah Agency Ltd", "999-999-999", new Guid("e1111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "id", "branch_id", "created_at", "created_by", "deleted_at", "deleted_by", "department_id", "email", "first_name", "is_active", "is_deleted", "last_name", "password_hash", "role", "tenant_id", "updated_at", "updated_by", "username" },
                values: new object[,]
                {
                    { new Guid("a1111111-1111-1111-1111-111111111111"), new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "admin@BusinessManagement.com", "System", true, false, "Administrator", "100000.YWRtaW5zYWx0MTIzNDU2Nw==.n4oZd6/RB84eYUOC5tNRItWB9SC1qWB4ikTZWrkn8dk=", "Admin", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "admin" },
                    { new Guid("a2222222-2222-2222-2222-222222222222"), new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "khaled@BusinessManagement.com", "Khaled", true, false, "Shareholder", "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=", "Partner", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "khaled" },
                    { new Guid("a3333333-3333-3333-3333-333333333333"), new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "alaa@BusinessManagement.com", "Alaa", true, false, "Shareholder", "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=", "Partner", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "alaa" },
                    { new Guid("a4444444-4444-4444-4444-444444444444"), new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "omar@BusinessManagement.com", "Omar", true, false, "Shareholder", "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=", "Partner", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "omar" },
                    { new Guid("a5555555-5555-5555-5555-555555555555"), new Guid("b1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", null, null, new Guid("d1111111-1111-1111-1111-111111111111"), "mohamed@BusinessManagement.com", "Mohamed", true, false, "Shareholder", "100000.cGFydG5lcnNhbHQxMjM0NQ==.Lqi0cCpbL4yektuVKlSyNa1IvRwmaIikRNzvyAP0bG8=", "Partner", new Guid("e1111111-1111-1111-1111-111111111111"), new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "system", "mohamed" }
                });

            migrationBuilder.InsertData(
                table: "branches",
                columns: new[] { "id", "company_id", "name" },
                values: new object[] { new Guid("b1111111-1111-1111-1111-111111111111"), new Guid("c1111111-1111-1111-1111-111111111111"), "Main Branch" });

            migrationBuilder.CreateIndex(
                name: "ix_branches_company_id",
                table: "branches",
                column: "company_id");

            migrationBuilder.CreateIndex(
                name: "ix_companies_tenant_id",
                table: "companies",
                column: "tenant_id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "branches");

            migrationBuilder.DropTable(
                name: "companies");

            migrationBuilder.DropTable(
                name: "tenants");

            migrationBuilder.DeleteData(
                table: "currencies",
                keyColumn: "id",
                keyValue: new Guid("e1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "currencies",
                keyColumn: "id",
                keyValue: new Guid("e2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("41111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "gateway_portals",
                keyColumn: "id",
                keyValue: new Guid("42222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "packages",
                keyColumn: "id",
                keyValue: new Guid("99999999-9999-9999-9999-999999999999"));

            migrationBuilder.DeleteData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "partners",
                keyColumn: "id",
                keyValue: new Guid("f4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("31111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("32222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("34444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "partnership_capitals",
                keyColumn: "id",
                keyValue: new Guid("35555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("51111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "safe_transactions",
                keyColumn: "id",
                keyValue: new Guid("52222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a1111111-1111-1111-1111-111111111111"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a2222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a3333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a4444444-4444-4444-4444-444444444444"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "id",
                keyValue: new Guid("a5555555-5555-5555-5555-555555555555"));

            migrationBuilder.DeleteData(
                table: "departments",
                keyColumn: "id",
                keyValue: new Guid("d1111111-1111-1111-1111-111111111111"));
        }
    }
}
