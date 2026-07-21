using C__Project.Data;
using C__Project.Models;
using Microsoft.EntityFrameworkCore;

namespace C__Project.Services;

public class FinancialAdvisorService
{
    private readonly ExpenseTrackerDbContext _context;

    public FinancialAdvisorService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public class AdvisorReport
    {
        public int HealthScore { get; set; } = 85;
        public string HealthLabel { get; set; } = "Healthy Pace";
        public string HealthBadgeColor { get; set; } = "#2ecc71";
        public List<AdvisoryTip> Tips { get; set; } = new();
    }

    public class AdvisoryTip
    {
        public string Type { get; set; } = "Tip"; // Danger, Warning, Tip, Success
        public string Icon { get; set; } = "💡";
        public string BadgeClass { get; set; } = "bg-primary";
        public string Category { get; set; } = "General";
        public string Title { get; set; } = "";
        public string Message { get; set; } = "";
    }

    public async Task<AdvisorReport> GenerateAdvisorReportAsync(SpendForecasterService.ForecastResult forecast)
    {
        var report = new AdvisorReport();
        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Fetch budgets and actual spend
        var budgets = await _context.Budgets
            .Include(b => b.Category)
            .Where(b => b.Month >= startOfMonth && b.Month <= endOfMonth)
            .AsNoTracking()
            .ToListAsync();

        var expenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= startOfMonth && e.Date <= endOfMonth)
            .AsNoTracking()
            .ToListAsync();

        decimal totalBudgetCap = budgets.Sum(b => b.Amount);
        decimal totalSpent = expenses.Sum(e => e.Amount);
        int daysInMonth = DateTime.DaysInMonth(today.Year, today.Month);
        int elapsedDays = Math.Max(today.Day, 1);
        int remainingDays = Math.Max(daysInMonth - elapsedDays, 0);

        int score = 100;
        var tips = new List<AdvisoryTip>();

        // 1. Overall Forecast Trajectory Check
        if (totalBudgetCap > 0)
        {
            decimal projectedSpend = forecast.ProjectedNextMonthTotal;
            if (projectedSpend > totalBudgetCap)
            {
                decimal excess = projectedSpend - totalBudgetCap;
                score -= 25;

                // Estimate day of month when budget is breached
                int breachDay = (int)Math.Min(daysInMonth, Math.Max(1, (double)totalBudgetCap / (double)(forecast.DailyBurnRate > 0 ? forecast.DailyBurnRate : 1)));

                tips.Add(new AdvisoryTip
                {
                    Type = "Danger",
                    Icon = "🚨",
                    BadgeClass = "badge-danger",
                    Category = "Budget Breach Risk",
                    Title = $"Projected to Exceed Budget by ${excess:N2}",
                    Message = $"At your current burn rate of ${forecast.DailyBurnRate:N2}/day, you are projected to reach your overall monthly limit by Day {breachDay}. Reduce daily non-essential spend by ${(excess / Math.Max(remainingDays, 1)):N2}/day to stay on track."
                });
            }
            else
            {
                decimal surplus = totalBudgetCap - projectedSpend;
                tips.Add(new AdvisoryTip
                {
                    Type = "Success",
                    Icon = "✅",
                    BadgeClass = "badge-success",
                    Category = "Monthly Surplus",
                    Title = $"On Track for ${surplus:N2} Monthly Savings",
                    Message = $"Your spending burn rate (${forecast.DailyBurnRate:N2}/day) is well within your monthly budget cap of ${totalBudgetCap:N2}. You are projected to finish the month with a healthy surplus."
                });
            }
        }

        // 2. High-Volume Category Auditing
        var categorySpends = expenses
            .GroupBy(e => e.Category)
            .Where(g => g.Key != null)
            .Select(g => new
            {
                Category = g.Key!,
                Amount = g.Sum(e => e.Amount)
            })
            .OrderByDescending(c => c.Amount)
            .ToList();

        if (totalSpent > 0 && categorySpends.Any())
        {
            var topSector = categorySpends.First();
            double sectorPct = (double)(topSector.Amount / totalSpent) * 100;

            if (sectorPct > 35.0)
            {
                score -= 10;
                tips.Add(new AdvisoryTip
                {
                    Type = "Warning",
                    Icon = "⚠️",
                    BadgeClass = "badge-warning",
                    Category = topSector.Category.Name,
                    Title = $"High Concentration in {topSector.Category.Icon} {topSector.Category.Name}",
                    Message = $"{topSector.Category.Name} represents {sectorPct:F1}% of your total cash outflow (${topSector.Amount:N2}). Trimming 15% from this sector will save ~${(topSector.Amount * 0.15m):N2} over the next {forecast.TargetDays} days."
                });
            }
        }

        // 3. Recurring Subscriptions Audit
        var subscriptions = expenses
            .Where(e => e.IsRecurring)
            .GroupBy(e => e.Description.Trim().ToLower())
            .Select(g => g.OrderByDescending(e => e.Date).First())
            .ToList();

        decimal monthlySubTotal = subscriptions.Sum(s => s.Amount);
        if (totalSpent > 0 && monthlySubTotal > 0)
        {
            double subPct = (double)(monthlySubTotal / totalSpent) * 100;
            if (subPct > 20.0)
            {
                score -= 10;
                tips.Add(new AdvisoryTip
                {
                    Type = "Tip",
                    Icon = "💡",
                    BadgeClass = "badge-info",
                    Category = "Subscriptions",
                    Title = $"Automated Subscriptions Account for {subPct:F0}% of Outflows",
                    Message = $"You have {subscriptions.Count} active recurring subscriptions totaling ${monthlySubTotal:N2}/mo. Audit unused memberships (e.g. streaming, gym) to free up automatic monthly cash flow."
                });
            }
        }

        // 4. Burn Rate Optimization Tip
        if (forecast.DailyBurnRate > 0)
        {
            decimal suggestedCap = Math.Max(10m, forecast.DailyBurnRate * 0.85m);
            tips.Add(new AdvisoryTip
            {
                Type = "Tip",
                Icon = "🧠",
                BadgeClass = "badge-primary",
                Category = "Burn Rate Optimizer",
                Title = $"Recommended Daily Cap: ${suggestedCap:N2}/day",
                Message = $"Cap your daily non-essential purchases at ${suggestedCap:N2}/day (a 15% reduction from current ${forecast.DailyBurnRate:N2}/day pace) to build an extra emergency savings cushion over the {forecast.TargetDays}-day horizon."
            });
        }

        // Finalize Score & Health Label
        score = Math.Clamp(score, 10, 100);
        report.HealthScore = score;

        if (score >= 85)
        {
            report.HealthLabel = "Optimal Pace";
            report.HealthBadgeColor = "#2ecc71"; // Green
        }
        else if (score >= 70)
        {
            report.HealthLabel = "Healthy Pace";
            report.HealthBadgeColor = "#3498db"; // Blue
        }
        else if (score >= 50)
        {
            report.HealthLabel = "Moderate Overspend Risk";
            report.HealthBadgeColor = "#f39c12"; // Amber
        }
        else
        {
            report.HealthLabel = "High Overspend Risk";
            report.HealthBadgeColor = "#e74c3c"; // Red
        }

        report.Tips = tips;
        return report;
    }
}
