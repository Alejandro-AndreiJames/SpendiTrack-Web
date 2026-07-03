using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SpendiTrackWeb.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMonthlyBudgetPeriod : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "UserBudgets",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "CategoryBudgets",
                type: "nvarchar(450)",
                maxLength: 450,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "UserBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "UserBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Year",
                table: "CategoryBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Month",
                table: "CategoryBudgets",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql(@"
                UPDATE UserBudgets
                SET Year = YEAR(UpdatedAt), Month = MONTH(UpdatedAt)
                WHERE Year = 0 AND Month = 0;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO UserBudgets (UserId, MonthlyIncome, SavingsPercent, FixedMonthlyCosts, UpdatedAt, Year, Month)
                SELECT ub.UserId, ub.MonthlyIncome, ub.SavingsPercent, ub.FixedMonthlyCosts, GETUTCDATE(), e.BudgetYear, e.BudgetMonth
                FROM UserBudgets ub
                INNER JOIN (
                    SELECT DISTINCT UserId, YEAR([Date]) AS BudgetYear, MONTH([Date]) AS BudgetMonth
                    FROM Expense
                ) e ON e.UserId = ub.UserId
                WHERE NOT EXISTS (
                    SELECT 1 FROM UserBudgets x
                    WHERE x.UserId = ub.UserId AND x.Year = e.BudgetYear AND x.Month = e.BudgetMonth
                );
            ");

            migrationBuilder.Sql(@"
                UPDATE cb
                SET cb.Year = ub.Year, cb.Month = ub.Month
                FROM CategoryBudgets cb
                INNER JOIN (
                    SELECT UserId, MIN(Year * 100 + Month) AS MinPeriodKey
                    FROM UserBudgets
                    GROUP BY UserId
                ) mp ON mp.UserId = cb.UserId
                INNER JOIN UserBudgets ub ON ub.UserId = mp.UserId AND (ub.Year * 100 + ub.Month) = mp.MinPeriodKey
                WHERE cb.Year = 0 AND cb.Month = 0;
            ");

            migrationBuilder.Sql(@"
                INSERT INTO CategoryBudgets (UserId, Category, AllocatedAmount, Year, Month)
                SELECT ub.UserId, cb.Category, cb.AllocatedAmount, ub.Year, ub.Month
                FROM UserBudgets ub
                INNER JOIN CategoryBudgets cb ON cb.UserId = ub.UserId
                WHERE NOT EXISTS (
                    SELECT 1 FROM CategoryBudgets x
                    WHERE x.UserId = ub.UserId
                      AND x.Year = ub.Year
                      AND x.Month = ub.Month
                      AND x.Category = cb.Category
                )
                AND (cb.Year * 100 + cb.Month) = (
                    SELECT MIN(c3.Year * 100 + c3.Month)
                    FROM CategoryBudgets c3
                    WHERE c3.UserId = ub.UserId
                );
            ");

            migrationBuilder.CreateIndex(
                name: "IX_UserBudgets_UserId_Year_Month",
                table: "UserBudgets",
                columns: new[] { "UserId", "Year", "Month" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CategoryBudgets_UserId_Year_Month_Category",
                table: "CategoryBudgets",
                columns: new[] { "UserId", "Year", "Month", "Category" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_UserBudgets_UserId_Year_Month",
                table: "UserBudgets");

            migrationBuilder.DropIndex(
                name: "IX_CategoryBudgets_UserId_Year_Month_Category",
                table: "CategoryBudgets");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "UserBudgets");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "UserBudgets");

            migrationBuilder.DropColumn(
                name: "Year",
                table: "CategoryBudgets");

            migrationBuilder.DropColumn(
                name: "Month",
                table: "CategoryBudgets");
        }
    }
}
