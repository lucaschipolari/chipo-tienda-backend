using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPricingCalculator : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "pricing_calculator_entries",
                schema: "catalog",
                columns: table => new
                {
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    CostUsdOrigin = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    PercentageCommission = table.Column<decimal>(type: "numeric(7,3)", nullable: false),
                    FixedCommission = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    ShippingCost = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    MarginType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    MarginValue = table.Column<decimal>(type: "numeric(14,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_pricing_calculator_entries", x => x.ProductId);
                    table.ForeignKey(
                        name: "FK_pricing_calculator_entries_products_ProductId",
                        column: x => x.ProductId,
                        principalSchema: "catalog",
                        principalTable: "products",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "pricing_calculator_entries",
                schema: "catalog");
        }
    }
}
