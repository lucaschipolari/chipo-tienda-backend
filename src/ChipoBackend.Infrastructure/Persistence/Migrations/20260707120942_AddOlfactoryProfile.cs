using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddOlfactoryProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "promotions",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "catalog",
                table: "products",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AddColumn<List<string>>(
                name: "BaseNotes",
                schema: "catalog",
                table: "products",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "HeartNotes",
                schema: "catalog",
                table: "products",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<int>(
                name: "Intensity",
                schema: "catalog",
                table: "products",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Longevity",
                schema: "catalog",
                table: "products",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<List<string>>(
                name: "Occasions",
                schema: "catalog",
                table: "products",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "Seasons",
                schema: "catalog",
                table: "products",
                type: "text[]",
                nullable: false);

            migrationBuilder.AddColumn<List<string>>(
                name: "TopNotes",
                schema: "catalog",
                table: "products",
                type: "text[]",
                nullable: false);

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "finance",
                table: "expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "Amount_Currency",
                schema: "expenses",
                table: "expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "discounts",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "coupons",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "ARS",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "PEN");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "BaseNotes",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "HeartNotes",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Intensity",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Longevity",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Occasions",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "Seasons",
                schema: "catalog",
                table: "products");

            migrationBuilder.DropColumn(
                name: "TopNotes",
                schema: "catalog",
                table: "products");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "promotions",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ARS");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "catalog",
                table: "products",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ARS");

            migrationBuilder.AlterColumn<string>(
                name: "currency",
                schema: "finance",
                table: "expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ARS");

            migrationBuilder.AlterColumn<string>(
                name: "Amount_Currency",
                schema: "expenses",
                table: "expenses",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ARS");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "discounts",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ARS");

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                schema: "promotions",
                table: "coupons",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "PEN",
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "ARS");
        }
    }
}
