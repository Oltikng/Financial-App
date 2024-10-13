using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace FinancialApp.Migrations
{
    /// <inheritdoc />
    public partial class AddIconColumnToAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Icon",
                table: "Accounts",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Icon",
                table: "Accounts");
        }
    }
}
