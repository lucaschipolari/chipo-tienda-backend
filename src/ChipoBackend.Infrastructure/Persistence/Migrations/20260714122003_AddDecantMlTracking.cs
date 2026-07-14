using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddDecantMlTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "BottleCost",
                schema: "catalog",
                table: "products",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "BottleMl",
                schema: "catalog",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDecant",
                schema: "catalog",
                table: "products",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "ReorderMl",
                schema: "catalog",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "StockMl",
                schema: "catalog",
                table: "products",
                type: "integer",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BottleCost",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "BottleMl",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "IsDecant",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "ReorderMl",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "StockMl",
                schema: "catalog",
                table: "products");
        }
    }
}
