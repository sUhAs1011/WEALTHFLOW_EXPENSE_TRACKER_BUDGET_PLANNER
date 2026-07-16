# 🪙 WEALTHFLOW_EXPENSE_TRACKER_BUDGET_PLANNER

A premium, modern, and responsive personal finance management dashboard built using **C#**, **ASP.NET Core Blazor Server**, and **Entity Framework Core with SQLite**. 

This application demonstrates modern enterprise C# web development patterns, showing how to build a clean architecture system with 100% C# frontend and backend code, zero-configuration local database storage, and beautiful responsive styling.

---

## 🚀 Key Features

* **📊 Interactive Dashboard**: A visually striking dark-themed dashboard presenting your monthly financial status (Total Spend, Active Budgets, and Recent Transactions) at a glance.
* **💸 Full-Featured Transaction Manager (CRUD)**: Log, edit, and delete transactions. Categorize expenses and filter or sort through history easily.
* **🎯 Smart Category Budgets**: Set spending limits on categories (e.g., Food, Entertainment, Transport). The dashboard dynamically updates progress bars and warns you if you are approaching or exceeding your budget.
* **📈 Visual Analytics**: View interactive, CSS/SVG-driven category distribution charts to see exactly where your money goes.
* **💾 Data Persistence**: Local SQLite file-based database integration with Entity Framework Core database migrations.

---

## 🛠️ Technology Stack

* **Core Framework**: .NET 8.0 / .NET 9.0 (ASP.NET Core Blazor Server)
* **ORM (Database Access)**: Entity Framework Core
* **Database**: SQLite (local file-based storage, no database server installation required)
* **Styling**: Premium custom CSS (Dark glassmorphism style, responsive grid, custom micro-animations)

---

## 🏗️ Project Architecture

The project follows clean coding practices and separation of concerns:

```text
WealthFlow/
│
├── Data/                       # Database Context & Migrations
│   ├── ExpenseTrackerDbContext.cs
│   └── Migrations/
│
├── Models/                     # Core Domain Entities
│   ├── Category.cs             # Expense Category (e.g., Food, Travel)
│   ├── Expense.cs              # Individual transactions
│   └── Budget.cs               # Monthly budget limits per category
│
├── Services/                   # Business Logic & DB Interactions
│   ├── ExpenseService.cs
│   └── BudgetService.cs
│
├── Pages/                      # Interactive Blazor UI Components
│   ├── Index.razor             # Dashboard home page
│   ├── Expenses.razor          # CRUD Interface for transactions
│   ├── Budgets.razor           # Configure monthly limits
│   └── Analytics.razor         # Financial breakdown graphs
│
└── wwwroot/                    # Static Assets & Styling
    └── css/
        └── site.css            # Custom CSS styles (Glassmorphism & animations)
```

---

## ⚡ Getting Started

### Prerequisites
Make sure you have the **.NET SDK (8.0 or later)** and **Visual Studio 2022** (with the *ASP.NET and web development* workload) installed.

### Installation & Run

1. **Scaffold the application** (or open the project folder in your IDE):
   ```bash
   cd WealthFlow
   ```

2. **Restore Dependencies & Build**:
   ```bash
   dotnet build
   ```

3. **Run the Application**:
   ```bash
   dotnet run
   ```
   Open your browser and navigate to `http://localhost:5000` (or the port specified in the terminal).

---

## 🔒 Database & Migrations

Entity Framework Core is set up to automatically create and seed the SQLite database (`wealthflow.db`) upon the first startup. 

To manually manage database schema changes, you can use the EF Core CLI:
* **Add a new migration**: `dotnet ef migrations add <MigrationName>`
* **Update database**: `dotnet ef database update`
