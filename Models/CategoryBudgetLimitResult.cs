namespace SpendiTrackWeb.Models
{
    public sealed class CategoryBudgetLimitResult
    {
        public bool IsAllowed { get; init; }
        public string? ErrorMessage { get; init; }

        public static CategoryBudgetLimitResult Allowed() =>
            new() { IsAllowed = true };

        public static CategoryBudgetLimitResult Denied(string message) =>
            new() { IsAllowed = false, ErrorMessage = message };
    }
}
