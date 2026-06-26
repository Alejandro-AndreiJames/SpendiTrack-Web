using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendiTrackWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseUserId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UserId",
                table: "Expense",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Expense");
        }
    }
}
