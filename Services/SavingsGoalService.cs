using C__Project.Data;
using C__Project.Models;
using Microsoft.EntityFrameworkCore;

namespace C__Project.Services;

public class SavingsGoalService
{
    private readonly ExpenseTrackerDbContext _context;

    public SavingsGoalService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    public class AIRecommendation
    {
        public decimal RequiredMonthlyContribution { get; set; }
        public int RemainingDays { get; set; }
        public int RemainingMonths { get; set; }
        public string RecommendationMessage { get; set; } = string.Empty;
        public bool IsOnTrack { get; set; } = true;
    }

    public async Task<List<SavingsGoal>> GetSavingsGoalsAsync()
    {
        return await _context.SavingsGoals
            .OrderBy(g => g.TargetDate)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<SavingsGoal?> GetSavingsGoalByIdAsync(int id)
    {
        return await _context.SavingsGoals.FindAsync(id);
    }

    public async Task<bool> SaveSavingsGoalAsync(SavingsGoal goal)
    {
        if (goal.Id == 0)
        {
            _context.SavingsGoals.Add(goal);
        }
        else
        {
            var existing = await _context.SavingsGoals.FindAsync(goal.Id);
            if (existing == null) return false;

            existing.Title = goal.Title;
            existing.TargetAmount = goal.TargetAmount;
            existing.CurrentAmount = goal.CurrentAmount;
            existing.TargetDate = goal.TargetDate;
            existing.Icon = goal.Icon;
            existing.Color = goal.Color;
            existing.Notes = goal.Notes;
            _context.SavingsGoals.Update(existing);
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DepositToGoalAsync(int goalId, decimal amount)
    {
        if (amount <= 0) return false;

        var goal = await _context.SavingsGoals.FindAsync(goalId);
        if (goal == null) return false;

        goal.CurrentAmount = Math.Min(goal.TargetAmount, goal.CurrentAmount + amount);
        _context.SavingsGoals.Update(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteSavingsGoalAsync(int id)
    {
        var goal = await _context.SavingsGoals.FindAsync(id);
        if (goal == null) return false;

        _context.SavingsGoals.Remove(goal);
        await _context.SaveChangesAsync();
        return true;
    }

    public AIRecommendation GetAIRecommendation(SavingsGoal goal, decimal dailyBurnRate)
    {
        var remainingGap = Math.Max(0m, goal.TargetAmount - goal.CurrentAmount);
        var remainingTime = goal.TargetDate - DateTime.Today;
        int remainingDays = Math.Max(1, (int)remainingTime.TotalDays);
        int remainingMonths = Math.Max(1, (int)Math.Ceiling(remainingDays / 30.44));

        decimal requiredMonthly = remainingGap / remainingMonths;
        decimal dailyPaceNeeded = remainingGap / remainingDays;

        bool isOnTrack = (dailyBurnRate <= 0) || (dailyPaceNeeded <= dailyBurnRate * 0.4m);

        string message;
        if (remainingGap <= 0)
        {
            message = "🎉 Goal Fully Achieved! Congratulations on reaching your target balance!";
        }
        else if (isOnTrack)
        {
            message = $"💡 AI Tip: Deposit ${requiredMonthly:N0}/mo (${dailyPaceNeeded:N2}/day) from surplus cash flow to reach target by {goal.TargetDate:MMM yyyy}.";
        }
        else
        {
            message = $"⚠️ Aggressive Target: Deposit ${requiredMonthly:N0}/mo needed. Trim non-essential expenses by ${dailyPaceNeeded:N2}/day to stay on schedule.";
        }

        return new AIRecommendation
        {
            RequiredMonthlyContribution = requiredMonthly,
            RemainingDays = remainingDays,
            RemainingMonths = remainingMonths,
            RecommendationMessage = message,
            IsOnTrack = isOnTrack
        };
    }
}
