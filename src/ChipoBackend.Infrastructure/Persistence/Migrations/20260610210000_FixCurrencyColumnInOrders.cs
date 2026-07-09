using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    [DbContext(typeof(AppDbContext))]
    [Migration("20260610210000_FixCurrencyColumnInOrders")]
    public partial class FixCurrencyColumnInOrders : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Use IF NOT EXISTS so this is safe to run even if the column already exists.
            migrationBuilder.Sql(@"
                ALTER TABLE orders.orders
                ADD COLUMN IF NOT EXISTS currency character varying(3) NOT NULL DEFAULT 'ARS';
            ");

            // Back-fill from the subtotal currency column where possible.
            migrationBuilder.Sql(@"
                UPDATE orders.orders
                SET currency = COALESCE(subtotal_currency, 'ARS')
                WHERE currency = 'ARS';
            ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE orders.orders DROP COLUMN IF EXISTS currency;
            ");
        }
    }
}
