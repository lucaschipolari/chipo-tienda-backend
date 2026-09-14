using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddSaleReferralAndDelivery : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                schema: "sales",
                table: "sales",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReferralSource",
                schema: "sales",
                table: "sales",
                type: "character varying(60)",
                maxLength: 60,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                schema: "sales",
                table: "sales");

            migrationBuilder.DropColumn(
                name: "ReferralSource",
                schema: "sales",
                table: "sales");
        }
    }
}
