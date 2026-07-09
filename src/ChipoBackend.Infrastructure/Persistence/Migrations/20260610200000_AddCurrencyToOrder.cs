using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260610200000_AddCurrencyToOrder")]
    /// <inheritdoc />
    public partial class AddCurrencyToOrder : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "currency",
                schema: "orders",
                table: "orders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS");

            // Update existing records: derive currency from the subtotal_currency column
            migrationBuilder.Sql(@"
                UPDATE orders.orders
                SET currency = COALESCE(subtotal_currency, 'ARS')
                WHERE currency = 'ARS' OR currency IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "currency",
                schema: "orders",
                table: "orders");
        }
    }
}
