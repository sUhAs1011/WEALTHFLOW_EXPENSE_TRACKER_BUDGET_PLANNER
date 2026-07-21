# 🪙 WEALTHFLOW — PERSONAL FINANCE TRACKER & BUDGET PLANNER

A premium, modern, and highly interactive personal finance management dashboard built using **C# 10 / .NET 10.0**, **ASP.NET Core Blazor Web App (Interactive Server)**, and **Entity Framework Core with SQLite**.

WealthFlow provides a beautiful dark-themed, glassmorphic interface to track expenses, manage budgets, analyze category breakdowns, and trace monthly financial commitments.

---

## 🚀 Key Features

* **🧠 Real-Time ML Auto-Categorization**: Powered by a zero-dependency, Laplace-smoothed Naive Bayes text classification algorithm written from scratch in pure C#. As you type a description when logging an expense (e.g., *"Uber ride"* or *"McDonalds"*), it dynamically calculates category probabilities and displays an interactive suggestion badge to auto-assign the category in one click.
* **📈 AI Spending Trend & 30-Day Forecaster**: Analyzes historical transaction trajectories using an **Ordinary Least Squares (OLS) Linear Regression** algorithm ($y = mx + c$). Generates projected 30-day totals, daily burn rate paces, and renders an interactive SVG chart featuring a solid actual line and a dashed projected trend line.
* **📊 Glowing "Quick Stats" KPI Row**: Four dynamic dashboard cards displaying **Total Monthly Spend**, **Remaining Budget balance** (turns red if over limit), **Highest Spending Sector**, and **Daily Average Burn Rate** with neon hover glows.
* **🔁 Recurring Subscriptions Tracker**: Toggle transactions as subscriptions (Netflix, Spotify, Rent). The Expenses panel features a dedicated sidebar showing total monthly committed drain, auto-billing dates, and transaction ledger badges.
* **📥 CSV Export Button**: Download your transaction logs as Excel-compatible `.csv` files. Filtering records dynamically exports only the currently searched subset, complete with a custom filename prompt.
* **📅 Date Range Presets**: Set filter calendar limits in one click using quick presets (**"This Month"**, **"Last 30 Days"**, **"This Year"**, **"All Time"**).
* **🎯 Category Limits & Progress Bars**: Set targets for monthly budgets. The Budgets view integrates category progress bars (Green/Yellow/Red alerts) and a master global status card showing total limits vs. actual spending.
* **📊 Visual Analytics & Donut Charts**: CSS and SVG-driven native donut charts and legend weights showing cash flow distribution across categories.
* **🔔 Thread-Safe Toast System**: Real-time top-right slide-in alerts confirming all transaction actions (Created/Updated/Deleted) and budget modifications.
* **📱 Horizontal Web Navbar**: Resized as a clean top header navbar containing active tab glows and responsive hamburger support for mobile layouts.

---

## 🛠️ Technology Stack

* **Core Framework**: .NET 10.0 (ASP.NET Core Blazor Web App)
* **Machine Learning & AI**: 
  - Pure C# Naive Bayes Text Classifier for auto-suggestions
  - Pure C# Ordinary Least Squares (OLS) Linear Regression for 30-day spend forecasting
  - Zero external packages or Python runtimes required
* **ORM (Database Access)**: Entity Framework Core 10.0
* **Database**: SQLite (Zero-configuration file-based DB, auto-generated on launch)
* **Styling**: Vanilla CSS (Fluid glassmorphism, responsive grids, CSS variables, and keyframe animations)

---

## 🏗️ Project Architecture

The project follows clean MVC/Service separation patterns:

```text
C#_Project/
│
├── Components/                 # UI Components and Pages
│   ├── Layout/
│   │   ├── MainLayout.razor    # Dashboard Shell & Toast Viewport
│   │   └── NavMenu.razor       # Top Horizontal Header Navigation
│   ├── Pages/
│   │   ├── Home.razor          # Main Dashboard & Budget Progress
│   │   ├── Expenses.razor      # Transaction CRUD, ML Suggestions & Subscriptions Tracker
│   │   ├── Budgets.razor       # Set limits & category creator
│   │   └── Analytics.razor     # SVG breakdown visualization & OLS AI Trend Chart
│   └── App.razor               # Blazor Entry, JS download helpers
│
├── Data/                       # Persistence Context
│   └── ExpenseTrackerDbContext.cs  # SQLite DbContext & Seeding
│
├── Models/                     # Core Domain Entities
│   ├── Category.cs             # Emoji & color tags
│   ├── Expense.cs              # Date, description, notes, and IsRecurring flag
│   └── Budget.cs               # Limit amount configuration
│
├── Services/                   # Database Queries & ML Logic
│   ├── CategoryService.cs
│   ├── ExpenseService.cs
│   ├── ExpenseClassifierService.cs # Pure C# Naive Bayes Text Classifier
│   └── SpendForecasterService.cs   # Pure C# OLS Linear Regression Forecaster
│
└── wwwroot/                    # Web Assets
    ├── app.css                 # Custom glassmorphic styles & animations
    └── favicon.png
```

---

## ⚡ Getting Started

### Prerequisites
Ensure you have the **.NET SDK (10.0 or later)** installed.

### Run Instructions (Windows AppLocker / Application Control)
If your environment has strict Application Control policies blocking executable binary execution inside the `Downloads` directory, compile and run the assembly DLL directly from a trusted directory (like `C:\Users\suhas\C#_Project`):

1. **Delete Existing SQLite DB** (required if resetting schema or seeding data):
   ```cmd
   del wealthflow.db
   ```

2. **Build the Application**:
   ```cmd
   dotnet build
   ```

3. **Start the DLL Host**:
   ```cmd
   dotnet bin\Debug\net10.0\C__Project.dll
   ```

4. **Navigate to App**:
   Open your browser to the local address outputted in the terminal console (usually `http://localhost:5000` or `https://localhost:5001`). 

5. **Hard Refresh**:
   Perform a hard reload (**`Ctrl + F5`**) in your browser to clear old CSS assets from the cache.
