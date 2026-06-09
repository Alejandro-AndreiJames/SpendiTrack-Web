using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendiTrackWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserBudget : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserBudgets",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UserId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    MonthlyIncome = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    SavingsPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    FixedMonthlyCosts = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserBudgets", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserBudgets");
        }
    }
}
