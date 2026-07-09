using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomerDocumentAndAddressFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Agregar DocumentType con valor por defecto "DNI" para registros existentes
            migrationBuilder.AddColumn<string>(
                name: "DocumentType",
                schema: "crm",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "DNI");

            // Hacer DocumentNumber NOT NULL con valor por defecto temporal para filas existentes
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

            // Dirección principal en el cliente
            migrationBuilder.AddColumn<string>(
                name: "Street",
                schema: "crm",
                table: "customers",
                type: "character varying(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "City",
                schema: "crm",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Province",
                schema: "crm",
                table: "customers",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                schema: "crm",
                table: "customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Índice único en DocumentNumber
            migrationBuilder.CreateIndex(
                name: "IX_customers_DocumentNumber",
                schema: "crm",
                table: "customers",
                column: "DocumentNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_customers_DocumentNumber",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "DocumentType",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Street",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "City",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "Province",
                schema: "crm",
                table: "customers");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                schema: "crm",
                table: "customers");

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
