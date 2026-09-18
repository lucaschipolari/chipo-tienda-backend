using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddProfitabilityModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "TargetMarginPct",
                schema: "catalog",
                table: "products",
                type: "numeric",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "product_cost_history",
                schema: "catalog",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false),
                    VariantId = table.Column<Guid>(type: "uuid", nullable: false),
                    unit_cost = table.Column<decimal>(type: "numeric(12,2)", nullable: false),
                    unit_cost_currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Source = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PurchaseOrderId = table.Column<Guid>(type: "uuid", nullable: true),
                    RecordedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_product_cost_history", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_product_cost_history_ProductId",
                schema: "catalog",
                table: "product_cost_history",
                column: "ProductId");

            migrationBuilder.CreateIndex(
                name: "IX_product_cost_history_VariantId",
                schema: "catalog",
                table: "product_cost_history",
                column: "VariantId");

            migrationBuilder.CreateIndex(
                name: "IX_product_cost_history_VariantId_RecordedAt",
                schema: "catalog",
                table: "product_cost_history",
                columns: new[] { "VariantId", "RecordedAt" });

            // Backfill: sembrar el historial de costos con las compras ya recibidas,
            // para que el módulo no arranque vacío. (No pisa ningún precio de venta.)
            migrationBuilder.Sql(@"
                INSERT INTO catalog.product_cost_history
                    (""Id"", ""ProductId"", ""VariantId"", unit_cost, unit_cost_currency, ""Source"", ""PurchaseOrderId"", ""RecordedAt"", ""CreatedByUserId"")
                SELECT gen_random_uuid(), i.""ProductId"", i.""VariantId"", i.unit_cost, i.unit_cost_currency,
                       'PurchaseReceipt', i.""PurchaseOrderId"", COALESCE(po.""UpdatedAt"", po.""CreatedAt""), po.""CreatedByUserId""
                FROM purchasing.purchase_order_items i
                JOIN purchasing.purchase_orders po ON po.""Id"" = i.""PurchaseOrderId""
                WHERE po.""Status"" IN ('Received', 'PartiallyReceived')
                  AND i.""QuantityReceived"" > 0;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "product_cost_history",
                schema: "catalog");

            migrationBuilder.DropColumn(
                name: "TargetMarginPct",
                schema: "catalog",
                table: "products");
        }
    }
}
