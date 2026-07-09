using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddPromotionsModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "discounts",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "promotions",
                table: "discounts",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "promotions",
                table: "discounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                schema: "promotions",
                table: "discounts",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "MinQuantity",
                schema: "promotions",
                table: "discounts",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Priority",
                schema: "promotions",
                table: "discounts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "coupons",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                schema: "promotions",
                table: "coupons",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                schema: "promotions",
                table: "coupons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsStackable",
                schema: "promotions",
                table: "coupons",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                schema: "promotions",
                table: "coupons",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "promotions",
                table: "coupon_usages",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                schema: "promotions",
                table: "coupon_usages",
                type: "numeric(12,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "coupon_restrictions",
                schema: "promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CouponId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    EntityId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_coupon_restrictions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_coupon_restrictions_coupons_CouponId",
                        column: x => x.CouponId,
                        principalSchema: "promotions",
                        principalTable: "coupons",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotions",
                schema: "promotions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Type = table.Column<string>(type: "character varying(30)", maxLength: 30, nullable: false),
                    Badge = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    StartsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    EndsAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActiveFrom = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    ActiveUntil = table.Column<TimeOnly>(type: "time without time zone", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    IsStackable = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Priority = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    DiscountType = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    DiscountValue = table.Column<decimal>(type: "numeric(8,4)", nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    BuyQuantity = table.Column<int>(type: "integer", nullable: true),
                    GetQuantity = table.Column<int>(type: "integer", nullable: true),
                    MinOrderAmount = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    ComboPrice = table.Column<decimal>(type: "numeric(12,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "promotion_categories",
                schema: "promotions",
                columns: table => new
                {
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_categories", x => new { x.PromotionId, x.CategoryId });
                    table.ForeignKey(
                        name: "FK_promotion_categories_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "promotions",
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "promotion_products",
                schema: "promotions",
                columns: table => new
                {
                    PromotionId = table.Column<Guid>(type: "uuid", nullable: false),
                    ProductId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_promotion_products", x => new { x.PromotionId, x.ProductId });
                    table.ForeignKey(
                        name: "FK_promotion_products_promotions_PromotionId",
                        column: x => x.PromotionId,
                        principalSchema: "promotions",
                        principalTable: "promotions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_coupon_restrictions_CouponId",
                schema: "promotions",
                table: "coupon_restrictions",
                column: "CouponId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "coupon_restrictions",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "promotion_categories",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "promotion_products",
                schema: "promotions");

            migrationBuilder.DropTable(
                name: "promotions",
                schema: "promotions");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "promotions",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "promotions",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "promotions",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                schema: "promotions",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "MinQuantity",
                schema: "promotions",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "Priority",
                schema: "promotions",
                table: "discounts");

            migrationBuilder.DropColumn(
                name: "Currency",
                schema: "promotions",
                table: "coupons");

            migrationBuilder.DropColumn(
                name: "Description",
                schema: "promotions",
                table: "coupons");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                schema: "promotions",
                table: "coupons");

            migrationBuilder.DropColumn(
                name: "IsStackable",
                schema: "promotions",
                table: "coupons");

            migrationBuilder.DropColumn(
                name: "Name",
                schema: "promotions",
                table: "coupons");

            migrationBuilder.DropColumn(
                name: "DiscountAmount",
                schema: "promotions",
                table: "coupon_usages");

            migrationBuilder.AlterColumn<Guid>(
                name: "CustomerId",
                schema: "promotions",
                table: "coupon_usages",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);
        }
    }
}
