using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialApp.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTransactionCategory : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Description",
                table: "Transactions");

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Category",
                table: "Transactions");

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "Transactions",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");
        }
    }
}
