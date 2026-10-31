# ExpenseTracker

**ASP.NET Core MVC · C# · Entity Framework Core · SQL Server**

ExpenseTracker is a server-rendered web application for recording expenses and allocating their amounts across multiple categories. It was built to explore relational data modeling, EF Core querying and change tracking, ASP.NET Core MVC model binding, and data aggregation with LINQ.

**Portfolio project #02 — ASP.NET Core MVC / EF Core**

## Overview

An expense is a single financial event: for example, a €100 purchase at a supermarket. That purchase can be split into several category allocations instead of being assigned to one category only:

| Expense | Category | Allocated amount |
| --- | --- | ---: |
| Weekly shopping · Lidl | Alimentari | €70.00 |
| Weekly shopping · Lidl | Casa | €30.00 |
| **Expense total** | | **€100.00** |

The total is **derived from its allocations**, rather than stored separately on the expense record.

## Features

- **Expense management:** create, view, edit, and delete expense records through ASP.NET Core MVC.
- **Multiple allocations per expense:** assign different amounts to different categories within the same transaction.
- **Dynamic Razor forms:** add and remove allocation rows without reloading the page; see the calculated total as amounts change.
- **Allocation-aware editing:** update existing allocations, add new ones, or remove allocations no longer needed.
- **Server-side validation:** reject non-positive amounts, unknown categories, and duplicate categories within one expense.
- **Expense overview:** list transactions with their category breakdowns and totals.
- **Category insights:** aggregate spending by category and display amounts and percentages.
- **Expense details:** inspect an individual transaction and its allocations.
- **Initial category seeding:** populate a standard set of categories through EF Core seeding.

## Tech stack

| Area | Technology |
| --- | --- |
| Language | C# |
| Web framework | ASP.NET Core MVC |
| Data access | Entity Framework Core |
| Database | SQL Server / SQL Server LocalDB |
| UI | Razor Views, Bootstrap 5 |
| Client-side interactions | Vanilla JavaScript |
| Database lifecycle | EF Core migrations and seeding |

## Data model

```text
Expense (1) ────── (*) ExpenseAllocation (*) ────── (1) Category
```

**Expense** stores the description, merchant, and occurrence date. **ExpenseAllocation** stores an amount and the foreign keys connecting an expense to a category. **Category** defines the spending classification shared by different expenses.

This design supports multiple categories for one expense, while keeping each category reusable across expenses.

## Implementation highlights

### EF Core relationships and querying

- `Include` / `ThenInclude` load allocations and their categories for the overview and detail pages.
- `AsNoTracking` is used for read-only queries.
- Tracked entities are updated during editing, allowing EF Core to persist changes to existing records.
- A single `SaveChangesAsync` call persists the relevant expense and allocation changes together.

### MVC forms and model binding

The Create and Edit forms use dedicated ViewModels rather than binding the whole persistence model directly. Dynamic allocation inputs are named using indexed paths such as:

```text
Allocations[0].CategoryId
Allocations[0].Amount
Allocations[1].CategoryId
Allocations[1].Amount
```

ASP.NET Core model binding reconstructs those inputs as a collection. During editing, allocation IDs distinguish existing records from newly added rows; missing existing allocations are removed from the expense.

### LINQ-based reporting

The overview uses LINQ operations such as `SelectMany`, `GroupBy`, `Sum`, and ordering to compute expense totals and category-level summaries from the loaded allocations.

### Category seeding

An idempotent `CategorySeeder` initializes the default categories through EF Core's `UseSeeding` configuration. Re-running the database update does not create duplicates of those initial category names.

## Project structure

The relevant application components are organized as follows:

```text
ExpenseTracker/
├── Controllers/
│   └── ExpensesController.cs
├── Data/
│   ├── ApplicationDbContext.cs
│   └── Seeders/
│       └── CategorySeeder.cs
├── Models/
│   ├── Expense.cs
│   ├── ExpenseAllocation.cs
│   └── Category.cs
├── ViewModels/
│   ├── CreateExpenseViewModel.cs
│   ├── EditExpenseViewModel.cs
│   └── ExpenseAllocationInputViewModel.cs
├── Views/
│   ├── Expenses/
│   │   ├── Index.cshtml
│   │   ├── Create.cshtml
│   │   ├── Edit.cshtml
│   │   ├── Details.cshtml
│   │   └── Delete.cshtml
│   └── Shared/
│       └── _ExpenseTrackerLayout.cshtml
├── Program.cs
└── appsettings.json
```

## Getting started

### Prerequisites

- A .NET SDK compatible with the target framework in the project file.
- Visual Studio with ASP.NET and web development tooling (recommended for this project).
- SQL Server LocalDB on Windows, or another accessible SQL Server instance.

### Setup

1. Clone the repository and open the solution in Visual Studio.
2. Restore the NuGet packages and check the `DefaultConnection` connection string in `appsettings.json`.
3. In **Tools → NuGet Package Manager → Package Manager Console**, select the web project as the default project and run:

   ```powershell
   Update-Database
   ```

   This applies the existing EF Core migrations and triggers the configured category seeding.
4. Run the application from Visual Studio and open **`/Expenses`**.

**Database configuration:** the project has been developed against SQL Server LocalDB. If you use another SQL Server instance, adjust `ConnectionStrings:DefaultConnection` before updating the database.

## Suggested manual verification

1. Create an expense with two allocations (for example, €70 in Alimentari and €30 in Casa). Check that the total is €100.
2. Edit the expense: change one amount, remove another allocation, and add a new category. Verify the resulting total and breakdown.
3. Submit duplicate categories or a non-positive amount and verify that the form is rejected.
4. Open the details page and confirm that every allocation is listed.
5. Check the overview's category summaries, then verify the Delete flow and associated allocation cleanup.

## Current scope and next improvements

This is a focused MVC/EF Core portfolio exercise, **not a production-ready personal finance product**. The current implementation has no authentication, per-user data isolation, or automated test suite documented here. The Delete flow and its associated allocation cleanup should receive a final end-to-end verification.

Potential follow-ups include automated MVC/EF Core integration tests, filtering by date or category, pagination, and more efficient database-side aggregations for larger datasets.

---

**What this project demonstrates:** ASP.NET Core MVC request handling, Razor forms and model binding, EF Core relational modeling and change tracking, server-side validation, and LINQ-based reporting.
