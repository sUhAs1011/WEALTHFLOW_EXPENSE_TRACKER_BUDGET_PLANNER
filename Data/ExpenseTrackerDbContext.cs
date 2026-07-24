using Microsoft.EntityFrameworkCore;
using C__Project.Models;

namespace C__Project.Data;

public class ExpenseTrackerDbContext : DbContext
{
    public ExpenseTrackerDbContext(DbContextOptions<ExpenseTrackerDbContext> options)
        : base(options)
    {
    }

    public DbSet<Category> Categories { get; set; } = null!;
    public DbSet<Expense> Expenses { get; set; } = null!;
    public DbSet<Budget> Budgets { get; set; } = null!;
    public DbSet<SavingsGoal> SavingsGoals { get; set; } = null!;
    public DbSet<PaymentMethod> PaymentMethods { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure Relationships
        modelBuilder.Entity<Expense>()
            .HasOne(e => e.Category)
            .WithMany(c => c.Expenses)
            .HasForeignKey(e => e.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Expense>()
            .HasOne(e => e.PaymentMethod)
            .WithMany(p => p.Expenses)
            .HasForeignKey(e => e.PaymentMethodId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Budget>()
            .HasOne(b => b.Category)
            .WithOne(c => c.Budget)
            .HasForeignKey<Budget>(b => b.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        // Seed Payment Methods
        var paymentMethods = new List<PaymentMethod>
        {
            new PaymentMethod { Id = 1, Name = "Main Checking", Type = PaymentType.Checking, Icon = "🏦", Color = "#3b82f6" },
            new PaymentMethod { Id = 2, Name = "Sapphire Rewards Card", Type = PaymentType.Credit, CreditLimit = 15000.00m, Icon = "💳", Color = "#8b5cf6" },
            new PaymentMethod { Id = 3, Name = "Cash Wallet", Type = PaymentType.Cash, Icon = "💵", Color = "#10b981" }
        };
        modelBuilder.Entity<PaymentMethod>().HasData(paymentMethods);

        // Seed Default Categories
        var categories = new List<Category>
        {
            new Category { Id = 1, Name = "Food & Dining", Icon = "🍔", Color = "#FF5733" },
            new Category { Id = 2, Name = "Transport & Fuel", Icon = "🚗", Color = "#33B5FF" },
            new Category { Id = 3, Name = "Utilities & Bills", Icon = "⚡", Color = "#F1C40F" },
            new Category { Id = 4, Name = "Entertainment", Icon = "🎬", Color = "#9B59B6" },
            new Category { Id = 5, Name = "Housing & Rent", Icon = "🏠", Color = "#2ECC71" },
            new Category { Id = 6, Name = "Shopping", Icon = "🛍️", Color = "#E91E63" }
        };
        modelBuilder.Entity<Category>().HasData(categories);

        // Seed Default Budgets (for the current month)
        var currentMonthStart = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
        modelBuilder.Entity<Budget>().HasData(
            new Budget { Id = 1, CategoryId = 1, Amount = 400.00m, Month = currentMonthStart }, // Food
            new Budget { Id = 2, CategoryId = 2, Amount = 150.00m, Month = currentMonthStart }, // Transport
            new Budget { Id = 3, CategoryId = 3, Amount = 300.00m, Month = currentMonthStart }, // Utilities
            new Budget { Id = 4, CategoryId = 5, Amount = 1200.00m, Month = currentMonthStart } // Rent
        );

        // Seed Sample Expenses
        modelBuilder.Entity<Expense>().HasData(
            new Expense { Id = 1, Description = "Groceries Weekly", Amount = 85.50m, Date = DateTime.Today.AddDays(-5), CategoryId = 1, Notes = "Whole Foods market", PaymentMethodId = 2 },
            new Expense { Id = 2, Description = "Gas Refill", Amount = 45.00m, Date = DateTime.Today.AddDays(-4), CategoryId = 2, Notes = "Chevron station", PaymentMethodId = 2 },
            new Expense { Id = 3, Description = "Electricity Bill", Amount = 120.40m, Date = DateTime.Today.AddDays(-3), CategoryId = 3, Notes = "Monthly power utility", PaymentMethodId = 1 },
            new Expense { Id = 4, Description = "Movie Tickets", Amount = 28.00m, Date = DateTime.Today.AddDays(-2), CategoryId = 4, Notes = "AMC Cinema", PaymentMethodId = 2 },
            new Expense { Id = 5, Description = "Apartment Rent", Amount = 1200.00m, Date = currentMonthStart, CategoryId = 5, Notes = "Bank transfer to landlord", IsRecurring = true, PaymentMethodId = 1 },
            new Expense { Id = 6, Description = "Sushi Dinner", Amount = 64.20m, Date = DateTime.Today.AddDays(-1), CategoryId = 1, Notes = "Date night at Sakura Sushi", PaymentMethodId = 2 },
            new Expense { Id = 7, Description = "Coffee & Bakery", Amount = 8.75m, Date = DateTime.Today, CategoryId = 1, Notes = "Starbucks morning break", PaymentMethodId = 3 }
        );

        // Seed Sample Savings Goals
        modelBuilder.Entity<SavingsGoal>().HasData(
            new SavingsGoal { Id = 1, Title = "Emergency Reserve Fund", TargetAmount = 5000.00m, CurrentAmount = 2400.00m, TargetDate = DateTime.Today.AddMonths(5), Icon = "🛡️", Color = "#2ecc71", Notes = "6 months liquid safety net" },
            new SavingsGoal { Id = 2, Title = "Tech Workstation Upgrade", TargetAmount = 2000.00m, CurrentAmount = 850.00m, TargetDate = DateTime.Today.AddMonths(3), Icon = "💻", Color = "#33B5FF", Notes = "M3 MacBook Pro or PC build" },
            new SavingsGoal { Id = 3, Title = "Summer Vacation", TargetAmount = 1500.00m, CurrentAmount = 300.00m, TargetDate = DateTime.Today.AddMonths(4), Icon = "🏖️", Color = "#F1C40F", Notes = "Flight & hotel booking" }
        );
    }
}
