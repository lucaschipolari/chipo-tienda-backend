using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ChipoBackend.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddExpensesModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ── Create expenses schema ────────────────────────────────────────
            migrationBuilder.EnsureSchema(name: "expenses");

            // ── expenses.expense_categories ──────────────────────────────────
            migrationBuilder.CreateTable(
                name: "expense_categories",
                schema: "expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Color = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false, defaultValue: "#6B7280"),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expense_categories", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expense_categories_Name",
                schema: "expenses",
                table: "expense_categories",
                column: "Name",
                unique: true);

            // ── expenses.expenses ─────────────────────────────────────────────
            migrationBuilder.CreateTable(
                name: "expenses",
                schema: "expenses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    Date = table.Column<DateTime>(type: "date", nullable: false),
                    Amount_Amount = table.Column<decimal>(type: "decimal(18,4)", nullable: false),
                    Amount_Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false, defaultValue: "PEN"),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    Observations = table.Column<string>(type: "text", nullable: true),
                    ReceiptUrl = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false, defaultValue: "Pending"),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedByUserId = table.Column<Guid>(type: "uuid", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_expenses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_expenses_expense_categories_CategoryId",
                        column: x => x.CategoryId,
                        principalSchema: "expenses",
                        principalTable: "expense_categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_expenses_CategoryId",
                schema: "expenses",
                table: "expenses",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_Date",
                schema: "expenses",
                table: "expenses",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_expenses_Status",
                schema: "expenses",
                table: "expenses",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "expenses",
                schema: "expenses");

            migrationBuilder.DropTable(
                name: "expense_categories",
                schema: "expenses");
        }
    }
}
