using SpendiTrackWeb.Models;

namespace SpendiTrackWeb.Services
{
    public class BudgetCalculator
    {
        /// <summary>
        /// Pure math: income, savings %, fixed costs to limit and remaining.
        /// </summary>
        public BudgetCalculationResult Calculate(
            decimal monthlyIncome,
            decimal savingsPercent,
            decimal fixedMonthlyCosts,
            decimal monthlySpent)
        {
            // Step 1: how much goes to savings
            decimal savingsAmount = monthlyIncome * (savingsPercent / 100m);

            // Step 2: what's left for variable spending
            decimal spendingLimit = monthlyIncome - savingsAmount - fixedMonthlyCosts;
            if (spendingLimit < 0)
                spendingLimit = 0;

            // Step 3: subtract what you already spent this month
            decimal remaining = spendingLimit - monthlySpent;

            return new BudgetCalculationResult
            {
                MonthlyIncome = monthlyIncome,
                SavingsPercent = savingsPercent,
                FixedMonthlyCosts = fixedMonthlyCosts,
                SavingsAmount = savingsAmount,
                SpendingLimit = spendingLimit,
                RemainingBudget = remaining
            };
        }

        /// <summary>
        /// Overload: when you already loaded a UserBudget from the database.
        /// </summary>
        public BudgetCalculationResult Calculate(UserBudget budget, decimal monthlySpent)
        {
            return Calculate(
                budget.MonthlyIncome,
                budget.SavingsPercent,
                budget.FixedMonthlyCosts,
                monthlySpent);
        }

        /// <summary>
        /// Copies calculated values into the page view model for Razor.
        /// </summary>
        public void ApplyToViewModel(BudgetCalculationResult result, ExpenseIndexViewModel viewModel)
        {
            viewModel.HasBudgetSetup = true;
            viewModel.MonthlyIncome = result.MonthlyIncome;
            viewModel.SavingsPercent = result.SavingsPercent;
            viewModel.FixedMonthlyCosts = result.FixedMonthlyCosts;
            viewModel.SpendingLimit = result.SpendingLimit;
            viewModel.RemainingBudget = result.RemainingBudget;
        }
    }
}