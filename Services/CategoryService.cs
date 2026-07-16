using Microsoft.EntityFrameworkCore;
using C__Project.Data;
using C__Project.Models;

namespace C__Project.Services;

public class CategoryService
{
    private readonly ExpenseTrackerDbContext _context;

    public CategoryService(ExpenseTrackerDbContext context)
    {
        _context = context;
    }

    // Get all categories, including their current budgets
    public async Task<List<Category>> GetCategoriesAsync()
    {
        return await _context.Categories
            .Include(c => c.Budget)
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Category?> GetCategoryByIdAsync(int id)
    {
        return await _context.Categories
            .Include(c => c.Budget)
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    // Save (Create or Update) Category
    public async Task<bool> SaveCategoryAsync(Category category)
    {
        try
        {
            if (category.Id == 0)
            {
                _context.Categories.Add(category);
            }
            else
            {
                var tracked = _context.Categories.Local.FirstOrDefault(c => c.Id == category.Id);
                if (tracked != null)
                {
                    _context.Entry(tracked).CurrentValues.SetValues(category);
                }
                else
                {
                    _context.Entry(category).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving category: {ex.Message}");
            return false;
        }
    }

    // Delete Category
    public async Task<bool> DeleteCategoryAsync(int id)
    {
        var category = await _context.Categories.FindAsync(id);
        if (category == null) return false;

        _context.Categories.Remove(category);
        return await _context.SaveChangesAsync() > 0;
    }

    // Save or Update a Budget for a Category
    public async Task<bool> SaveBudgetAsync(Budget budget)
    {
        try
        {
            if (budget.Id == 0)
            {
                // Check if budget for this category and month already exists
                var existingBudget = await _context.Budgets
                    .FirstOrDefaultAsync(b => b.CategoryId == budget.CategoryId && 
                                              b.Month.Year == budget.Month.Year && 
                                              b.Month.Month == budget.Month.Month);
                
                if (existingBudget != null)
                {
                    existingBudget.Amount = budget.Amount;
                    _context.Budgets.Update(existingBudget);
                }
                else
                {
                    _context.Budgets.Add(budget);
                }
            }
            else
            {
                var tracked = _context.Budgets.Local.FirstOrDefault(b => b.Id == budget.Id);
                if (tracked != null)
                {
                    _context.Entry(tracked).CurrentValues.SetValues(budget);
                }
                else
                {
                    _context.Entry(budget).State = EntityState.Modified;
                }
            }

            await _context.SaveChangesAsync();
            return true;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error saving budget: {ex.Message}");
            return false;
        }
    }

    // Delete Budget
    public async Task<bool> DeleteBudgetAsync(int budgetId)
    {
        var budget = await _context.Budgets.FindAsync(budgetId);
        if (budget == null) return false;

        _context.Budgets.Remove(budget);
        return await _context.SaveChangesAsync() > 0;
    }
}
