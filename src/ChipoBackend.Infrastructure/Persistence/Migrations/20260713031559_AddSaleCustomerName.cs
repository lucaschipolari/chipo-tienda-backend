using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleCustomerName : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CustomerName",
                schema: "sales",
                table: "sales",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CustomerName",
                schema: "sales",
                table: "sales");
        }
    }
}
