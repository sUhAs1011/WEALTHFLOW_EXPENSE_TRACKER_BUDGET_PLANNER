using Microsoft.EntityFrameworkCore;
using C__Project.Data;
using C__Project.Models;

namespace C__Project.Services;

public class ExpenseService
{
    private readonly ExpenseTrackerDbContext _context;

    public ExpenseService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    // Get all expenses, including their associated Category
    public async Task<List<Expense>> GetExpensesAsync()
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .AsNoTracking()
            .OrderByDescending(e => e.Date)
            .ToListAsync();
    }

    // Get expenses with filtering and searching
    public async Task<List<Expense>> GetExpensesFilteredAsync(string? search, int? categoryId, DateTime? startDate, DateTime? endDate)
    {
        var query = _context.Expenses.Include(e => e.Category).AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(e => EF.Functions.Like(e.Description, $"%{search}%") || 
                                     (e.Notes != null && EF.Functions.Like(e.Notes, $"%{search}%")));
        }

        if (categoryId.HasValue && categoryId.Value > 0)
        {
            query = query.Where(e => e.CategoryId == categoryId.Value);
        }

        if (startDate.HasValue)
        {
            query = query.Where(e => e.Date >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(e => e.Date <= endDate.Value);
        }

        return await query.OrderByDescending(e => e.Date).ToListAsync();
    }

    public async Task<Expense?> GetExpenseByIdAsync(int id)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Id == id);
    }

    // Save (Create or Update) Expense
    public async Task<bool> SaveExpenseAsync(Expense expense)
    {
        try
        {
            if (expense.Id == 0)
            {
                _context.Expenses.Add(expense);
            }
            else
            {
                var tracked = _context.Expenses.Local.FirstOrDefault(e => e.Id == expense.Id);
                if (tracked != null)
                {
                    _context.Entry(tracked).CurrentValues.SetValues(expense);
                }
                else
                {
                    _context.Entry(expense).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving expense: {ex.Message}");
            return false;
        }
    }

    // Delete Expense
    public async Task<bool> DeleteExpenseAsync(int id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense == null) return false;

        _context.Expenses.Remove(expense);
        return await _context.SaveChangesAsync() > 0;
    }

    // Get total expenses for a specific month and year
    public async Task<decimal> GetTotalExpensesForMonthAsync(int year, int month)
    {
        return await _context.Expenses
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .SumAsync(e => e.Amount);
    }

    // Get aggregate expenses grouped by category for a specific month and year
    public async Task<List<CategorySpend>> GetExpensesByCategoryForMonthAsync(int year, int month)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .Where(e => e.Date.Year == year && e.Date.Month == month)
            .GroupBy(e => new { e.Category!.Name, e.Category.Color, e.Category.Icon })
            .Select(g => new CategorySpend
            {
                CategoryName = g.Key.Name,
                Color = g.Key.Color,
                Icon = g.Key.Icon,
                Amount = g.Sum(e => e.Amount)
            })
            .OrderByDescending(x => x.Amount)
            .ToListAsync();
    }

    // Get N most recent expenses
    public async Task<List<Expense>> GetRecentExpensesAsync(int count)
    {
        return await _context.Expenses
            .Include(e => e.Category)
            .AsNoTracking()
            .OrderByDescending(e => e.Date)
            .Take(count)
            .ToListAsync();
    }
}

// Helper class for analytics visualization
public class CategorySpend
{
    public string CategoryName { get; set; } = string.Empty;
    public string Color { get; set; } = "#cccccc";
    public string Icon { get; set; } = "📁";
    public decimal Amount { get; set; }
}
