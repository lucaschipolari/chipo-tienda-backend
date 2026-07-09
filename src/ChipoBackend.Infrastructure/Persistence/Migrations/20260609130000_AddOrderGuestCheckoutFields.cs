using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOrderGuestCheckoutFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Make CustomerId nullable
            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: false);

            // Add BuyerName
            migrationBuilder.AddColumn<string>(
                name: "BuyerName",
                schema: "orders",
                table: "orders",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            // Add BuyerEmail
            migrationBuilder.AddColumn<string>(
                name: "BuyerEmail",
                schema: "orders",
                table: "orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "");

            // Add BuyerPhone
            migrationBuilder.AddColumn<string>(
                name: "BuyerPhone",
                schema: "orders",
                table: "orders",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            // Add PaymentMethod
            migrationBuilder.AddColumn<string>(
                name: "PaymentMethod",
                schema: "orders",
                table: "orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            // Add DeliveryMethod
            migrationBuilder.AddColumn<string>(
                name: "DeliveryMethod",
                schema: "orders",
                table: "orders",
                type: "character varying(30)",
                maxLength: 30,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BuyerName",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "BuyerEmail",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "BuyerPhone",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "PaymentMethod",
                schema: "orders",
                table: "orders");

            migrationBuilder.DropColumn(
                name: "DeliveryMethod",
                schema: "orders",
                table: "orders");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "orders",
                table: "orders",
                type: "uuid",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
