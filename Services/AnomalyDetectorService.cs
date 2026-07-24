using C__Project.Data;
using Microsoft.EntityFrameworkCore;

namespace C__Project.Services;

public class AnomalyDetectorService
{
    private readonly ExpenseTrackerDbContext _context;

    public AnomalyDetectorService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public class AnomalyResult
    {
        public bool IsAnomaly { get; set; }
        public double ZScore { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal StandardDeviation { get; set; }
        public int TotalCount { get; set; }
    }

    public async Task<AnomalyResult> AnalyzeAmountAsync(int categoryId, decimal amount)
    {
        var result = new AnomalyResult();

        if (categoryId <= 0 || amount <= 0) return result;

        // Fetch historical expenses in the same category
        var pastExpenses = await _context.Expenses
            .Where(e => e.CategoryId == categoryId)
            .Select(e => (double)e.Amount)
            .AsNoTracking()
            .ToListAsync();

        result.TotalCount = pastExpenses.Count;

        // Insufficient data fallback
        if (pastExpenses.Count < 3)
        {
            // If category budget is available, use it for scale
            var budget = await _context.Budgets
                .Where(b => b.CategoryId == categoryId)
                .Select(b => (double)b.Amount)
                .AsNoTracking()
                .FirstOrDefaultAsync();

            double baseVal = budget > 0 ? budget / 5.0 : 50.0;
            double diff = (double)amount - baseVal;
            
            result.IsAnomaly = diff > baseVal * 2.0;
            result.ZScore = diff > 0 ? diff / baseVal : 0.0;
            result.AverageAmount = (decimal)baseVal;
            result.StandardDeviation = (decimal)(baseVal / 2.0);
            return result;
        }

        // Calculate Mean (μ)
        double mean = pastExpenses.Average();
        result.AverageAmount = (decimal)mean;

        // Calculate Standard Deviation (σ)
        double sumOfSquares = pastExpenses.Sum(val => Math.Pow(val - mean, 2));
        double stdDev = Math.Sqrt(sumOfSquares / pastExpenses.Count);
        result.StandardDeviation = (decimal)stdDev;

        // Compute Z-Score
        if (stdDev > 0.1)
        {
            double z = ((double)amount - mean) / stdDev;
            result.ZScore = z;
            // Flag as anomaly if Z-Score exceeds threshold of 2.0 standard deviations
            result.IsAnomaly = z > 2.0;
        }
        else
        {
            // Fallback for zero/near-zero variance
            result.IsAnomaly = (double)amount > (mean * 2.5);
            result.ZScore = (double)amount > mean ? ((double)amount - mean) / (mean * 0.5) : 0.0;
        }

        return result;
    }
}
