using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddVariantCompareAtPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "compare_at_currency",
                schema: "catalog",
                table: "product_variants",
                type: "character varying(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "compare_at_price",
                schema: "catalog",
                table: "product_variants",
                type: "numeric(12,2)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "compare_at_currency",
                schema: "catalog",
                table: "product_variants");

            migrationBuilder.DropColumn(
                name: "compare_at_price",
                schema: "catalog",
                table: "product_variants");
        }
    }
}
