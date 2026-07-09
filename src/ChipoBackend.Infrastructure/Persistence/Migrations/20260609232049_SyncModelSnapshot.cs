using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class SyncModelSnapshot : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "country",
                schema: "purchasing",
                table: "suppliers",
                newName: "Country");

            migrationBuilder.RenameColumn(
                name: "city",
                schema: "purchasing",
                table: "suppliers",
                newName: "City");

            migrationBuilder.AlterColumn<string>(
                name: "TaxId",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentTerms",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactName",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

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
                name: "address_city",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "address_country",
                schema: "purchasing",
                table: "suppliers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "purchasing",
                table: "purchase_orders",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "purchasing",
                table: "purchase_orders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<string>(
                name: "BuyerEmail",
                schema: "orders",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                schema: "orders",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                schema: "orders",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                schema: "orders",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "orders",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "crm",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "crm",
                table: "customers",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                schema: "crm",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "crm",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                schema: "crm",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "crm",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "crm",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Street",
                schema: "crm",
                table: "customers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

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
                    IsPrimary = table.Column<bool>(type: "boolean", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "supplier_products",
                schema: "purchasing",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    SupplierProductCode = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    PurchasePrice = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    LeadTimeDays = table.Column<int>(type: "integer", nullable: false),
                    IsPreferredSupplier = table.Column<bool>(type: "boolean", nullable: false),
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
                name: "IX_customers_DocumentNumber",
                schema: "crm",
                table: "customers",
                column: "DocumentNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_supplier_contacts_SupplierId",
                schema: "purchasing",
                table: "supplier_contacts",
                column: "SupplierId");

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

            migrationBuilder.DropIndex(
                name: "IX_customers_DocumentNumber",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "TradeName",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Website",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "address_city",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "address_country",
                schema: "purchasing",
                table: "suppliers");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "purchasing",
                table: "purchase_orders");

            migrationBuilder.DropColumn(
                name: "BuyerEmail",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "BuyerName",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Street",
                schema: "crm",
                table: "customers");

            migrationBuilder.RenameColumn(
                name: "Country",
                schema: "purchasing",
                table: "suppliers",
                newName: "country");

            migrationBuilder.RenameColumn(
                name: "City",
                schema: "purchasing",
                table: "suppliers",
                newName: "city");

            migrationBuilder.AlterColumn<string>(
                name: "TaxId",
                schema: "purchasing",
                table: "suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Phone",
                schema: "purchasing",
                table: "suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PaymentTerms",
                schema: "purchasing",
                table: "suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "purchasing",
                table: "suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                schema: "purchasing",
                table: "suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ContactName",
                schema: "purchasing",
                table: "suppliers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "purchasing",
                table: "purchase_orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                schema: "crm",
                table: "customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Notes",
                schema: "crm",
                table: "customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "DocumentNumber",
                schema: "crm",
                table: "customers",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);
        }
    }
}
