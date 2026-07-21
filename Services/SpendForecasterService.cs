using C__Project.Data;
using C__Project.Models;
using Microsoft.EntityFrameworkCore;

namespace C__Project.Services;

public class SpendForecasterService
{
    private readonly ExpenseTrackerDbContext _context;

    public SpendForecasterService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public class ForecastResult
    {
        public decimal CurrentMonthSpend { get; set; }
        public decimal ProjectedNextMonthTotal { get; set; }
        public decimal DailyBurnRate { get; set; }
        public double TrendPercentage { get; set; }
        public int TargetDays { get; set; } = 30;
        public string ConfidenceLevel { get; set; } = "Medium";
        public string TopGrowthCategoryName { get; set; } = "";
        public string TopGrowthCategoryIcon { get; set; } = "📈";
        public List<DailyDataPoint> DailyPoints { get; set; } = new();
    }

    public class DailyDataPoint
    {
        public int Day { get; set; }
        public decimal ActualCumulative { get; set; }
        public decimal ProjectedCumulative { get; set; }
        public bool IsProjection { get; set; }
    }

    public async Task<ForecastResult> GenerateForecastAsync(int targetDays = 30)
    {
        if (targetDays < 1) targetDays = 30;

        var today = DateTime.Today;
        var startOfMonth = new DateTime(today.Year, today.Month, 1);
        var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);

        // Query current month expenses
        var currentExpenses = await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date >= startOfMonth && e.Date <= endOfMonth)
            .OrderBy(e => e.Date)
            .AsNoTracking()
            .ToListAsync();

        decimal currentMonthSpend = currentExpenses.Sum(e => e.Amount);
        int elapsedDays = Math.Max(today.Day, 1);

        // Group daily actual spend
        var dailyActualMap = currentExpenses
            .GroupBy(e => e.Date.Day)
            .ToDictionary(g => g.Key, g => g.Sum(e => e.Amount));

        // Prepare points for OLS linear regression (x = day, y = cumulative spend)
        var xValues = new List<double>();
        var yValues = new List<double>();

        double cumulative = 0.0;
        for (int day = 1; day <= elapsedDays; day++)
        {
            if (dailyActualMap.TryGetValue(day, out var dayAmount))
            {
                cumulative += (double)dayAmount;
            }
            xValues.Add(day);
            yValues.Add(cumulative);
        }

        // Perform OLS Linear Regression y = m * x + c
        double m = 0.0;
        double c = 0.0;
        double rSquared = 0.0;

        if (xValues.Count >= 2)
        {
            double sumX = xValues.Sum();
            double sumY = yValues.Sum();
            double sumXY = xValues.Zip(yValues, (x, y) => x * y).Sum();
            double sumX2 = xValues.Sum(x => x * x);
            int n = xValues.Count;

            double denominator = (n * sumX2 - sumX * sumX);
            if (Math.Abs(denominator) > 0.0001)
            {
                m = (n * sumXY - sumX * sumY) / denominator;
                c = (sumY - m * sumX) / n;

                // Ensure non-negative slope
                m = Math.Max(m, 0.0);

                // Calculate R² (Coefficient of Determination)
                double yMean = sumY / n;
                double totalSS = yValues.Sum(y => Math.Pow(y - yMean, 2));
                double resSS = xValues.Zip(yValues, (x, y) => Math.Pow(y - (m * x + c), 2)).Sum();
                rSquared = totalSS > 0 ? Math.Max(0.0, 1.0 - (resSS / totalSS)) : 1.0;
            }
        }
        else if (elapsedDays == 1 && currentMonthSpend > 0)
        {
            m = (double)currentMonthSpend;
            c = 0.0;
            rSquared = 0.5;
        }

        // Calculate projections for N target days
        decimal dailyBurnRate = (decimal)Math.Max(m, (double)currentMonthSpend / elapsedDays);
        decimal projectedNextMonthTotal = dailyBurnRate * targetDays;

        double trendPercentage = currentMonthSpend > 0 
            ? ((double)(projectedNextMonthTotal - currentMonthSpend) / (double)currentMonthSpend) * 100 
            : 0.0;

        string confidenceLevel = rSquared >= 0.75 ? "High" : rSquared >= 0.4 ? "Medium" : "Low";

        // Determine top growth category
        var categoryTotals = currentExpenses
            .GroupBy(e => e.Category)
            .Where(g => g.Key != null)
            .OrderByDescending(g => g.Sum(e => e.Amount))
            .FirstOrDefault();

        string topCatName = categoryTotals?.Key?.Name ?? "General";
        string topCatIcon = categoryTotals?.Key?.Icon ?? "📈";

        // Build N-day graph data points
        var points = new List<DailyDataPoint>();
        double runningActual = 0.0;

        for (int day = 1; day <= targetDays; day++)
        {
            bool isProjection = day > elapsedDays;
            if (day <= elapsedDays && dailyActualMap.TryGetValue(day, out var amt))
            {
                runningActual += (double)amt;
            }

            double projectedVal = m * day + c;
            if (projectedVal < runningActual && !isProjection)
            {
                projectedVal = runningActual;
            }

            points.Add(new DailyDataPoint
            {
                Day = day,
                ActualCumulative = (decimal)runningActual,
                ProjectedCumulative = (decimal)Math.Max(projectedVal, 0),
                IsProjection = isProjection
            });
        }

        return new ForecastResult
        {
            CurrentMonthSpend = currentMonthSpend,
            ProjectedNextMonthTotal = projectedNextMonthTotal,
            DailyBurnRate = dailyBurnRate,
            TrendPercentage = trendPercentage,
            TargetDays = targetDays,
            ConfidenceLevel = confidenceLevel,
            TopGrowthCategoryName = topCatName,
            TopGrowthCategoryIcon = topCatIcon,
            DailyPoints = points
        };
    }
}
