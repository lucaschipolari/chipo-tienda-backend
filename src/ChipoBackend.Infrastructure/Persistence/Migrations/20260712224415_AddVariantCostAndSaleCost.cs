using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantCostAndSaleCost : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "unit_cost",
                schema: "sales",
                table: "sale_items",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "unit_cost_currency",
                schema: "sales",
                table: "sale_items",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<decimal>(
                name: "cost",
                schema: "catalog",
                table: "product_variants",
                type: "numeric(12,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "cost_currency",
                schema: "catalog",
                table: "product_variants",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "unit_cost",
                schema: "sales",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "unit_cost_currency",
                schema: "sales",
                table: "sale_items");

            migrationBuilder.DropColumn(
                name: "cost",
                schema: "catalog",
                table: "product_variants");

            migrationBuilder.DropColumn(
                name: "cost_currency",
                schema: "catalog",
                table: "product_variants");
        }
    }
}
