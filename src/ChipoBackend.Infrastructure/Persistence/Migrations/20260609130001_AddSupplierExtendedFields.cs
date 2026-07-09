using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSupplierExtendedFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Nuevas columnas en purchasing.suppliers ────────────────────────

            migrationBuilder.AddColumn<string>(
                name: "TradeName",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Website",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Country",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                defaultValue: "Peru");

            // ── Nueva columna Currency en purchasing.purchase_orders ───────────

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "purchasing",
                table: "purchase_orders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN");

            // ── Tabla purchasing.supplier_contacts ────────────────────────────

            migrationBuilder.CreateTable(
                name: "supplier_contacts",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    JobTitle = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    Email = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: true),
                    Phone = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_contacts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_contacts_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "purchasing",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_contacts_SupplierId",
                schema: "purchasing",
                table: "supplier_contacts",
                column: "SupplierId");

            // ── Tabla purchasing.supplier_products ────────────────────────────

            migrationBuilder.CreateTable(
                name: "supplier_products",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "decimal(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsPreferredSupplier = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_supplier_products_suppliers_SupplierId",
                        column: x => x.SupplierId,
                        principalSchema: "purchasing",
                        principalTable: "suppliers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_products_SupplierId_ProductId",
                schema: "purchasing",
                table: "supplier_products",
                columns: new[] { "SupplierId", "ProductId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_contacts",
                schema: "purchasing");

            migrationBuilder.DropTable(
                name: "supplier_products",
                schema: "purchasing");

            migrationBuilder.DropColumn(
                name: "TradeName",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Country",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "purchasing",
                table: "purchase_orders");
        }
    }
}
