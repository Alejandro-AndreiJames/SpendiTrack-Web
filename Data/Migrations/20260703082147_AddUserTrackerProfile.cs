using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendiTrackWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTrackerProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTrackerProfiles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "nvarchar(450)", maxLength: 450, nullable: false),
                    StartYear = table.Column<int>(type: "int", nullable: false),
                    StartMonth = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTrackerProfiles", x => x.UserId);
                });

            // Year/Month on UserBudgets are added in the next migration — use UpdatedAt here.
            migrationBuilder.Sql(@"
                INSERT INTO UserTrackerProfiles (UserId, StartYear, StartMonth)
                SELECT
                    u.Id,
                    periodKey / 100,
                    periodKey % 100
                FROM AspNetUsers u
                CROSS APPLY (
                    SELECT COALESCE(
                        (SELECT MIN(YEAR(ub.UpdatedAt) * 100 + MONTH(ub.UpdatedAt)) FROM UserBudgets ub WHERE ub.UserId = u.Id AND ub.MonthlyIncome > 0),
                        (SELECT MIN(YEAR(ub.UpdatedAt) * 100 + MONTH(ub.UpdatedAt)) FROM UserBudgets ub WHERE ub.UserId = u.Id),
                        (SELECT MIN(YEAR(e.[Date]) * 100 + MONTH(e.[Date])) FROM Expense e WHERE e.UserId = u.Id),
                        YEAR(GETUTCDATE()) * 100 + MONTH(GETUTCDATE())
                    ) AS periodKey
                ) p;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTrackerProfiles");
        }
    }
}
