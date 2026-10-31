
using ExpenseTracker.Data;
using ExpenseTracker.Models;
using ExpenseTracker.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;

public class ExpensesController : Controller
{
    private readonly ApplicationDbContext _context;

    public ExpensesController(ApplicationDbContext context)
    {
        _context = context;
    }

    // GET: EXPENSES
    public async Task<IActionResult> Index()
    {
        var expenses = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Allocations)
                .ThenInclude(a => a.Category)
            .OrderByDescending(e => e.OccurredOn)
            .ToListAsync();

        return View(expenses);
    }

    // GET: EXPENSES/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var expense = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Allocations)
                .ThenInclude(a => a.Category)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
            return NotFound();

        return View(expense);
    }

    // GET: EXPENSES/Create
    public async Task<IActionResult> Create()
    {
        var model = new CreateExpenseViewModel
        {
            OccurredOn = DateOnly.FromDateTime(DateTime.Today),

            Allocations = new List<ExpenseAllocationInputViewModel>
        {
            new()
        },

            Categories = await GetCategoryOptionsAsync()
        };

        return View(model);
    }

    // POST: EXPENSES/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CreateExpenseViewModel model)
    {
        if (model.Allocations.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.Allocations),
                "Inserisci almeno una ripartizione.");
        }

        var validCategoryIds = await _context.Categories
            .Select(c => c.Id)
            .ToListAsync();

        var selectedCategories = new HashSet<int>();

        for (int i = 0; i < model.Allocations.Count; i++)
        {
            var allocation = model.Allocations[i];

            if (allocation.Amount <= 0)
            {
                ModelState.AddModelError(
                    $"Allocations[{i}].Amount",
                    "L'importo deve essere maggiore di zero.");
            }

            if (!validCategoryIds.Contains(allocation.CategoryId))
            {
                ModelState.AddModelError(
                    $"Allocations[{i}].CategoryId",
                    "Seleziona una categoria valida.");
            }
            else if (!selectedCategories.Add(allocation.CategoryId))
            {
                ModelState.AddModelError(
                    $"Allocations[{i}].CategoryId",
                    "Questa categoria è già presente.");
            }
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync();

            if (model.Allocations.Count == 0)
            {
                model.Allocations.Add(new ExpenseAllocationInputViewModel());
            }

            return View(model);
        }

        var expense = new Expense
        {
            Description = model.Description,
            Merchant = model.Merchant,
            OccurredOn = model.OccurredOn
        };

        foreach (var allocation in model.Allocations)
        {
            expense.Allocations.Add(new ExpenseAllocation
            {
                CategoryId = allocation.CategoryId,
                Amount = allocation.Amount
            });
        }

        _context.Expenses.Add(expense);
        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }

    private Task<List<SelectListItem>> GetCategoryOptionsAsync()
    {
        return _context.Categories
            .AsNoTracking()
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name
            })
            .ToListAsync();
    }

    // GET: EXPENSES/Edit/5


    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var expense = await _context.Expenses
            .AsNoTracking()
            .Include(e => e.Allocations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
            return NotFound();

        var model = new EditExpenseViewModel
        {
            Id = expense.Id,
            Description = expense.Description,
            Merchant = expense.Merchant,
            OccurredOn = expense.OccurredOn,

            Allocations = expense.Allocations
                .OrderBy(a => a.Id)
                .Select(a => new ExpenseAllocationInputViewModel
                {
                    Id = a.Id,
                    CategoryId = a.CategoryId,
                    Amount = a.Amount
                })
                .ToList(),

            Categories = await GetCategoryOptionsAsync()
        };

        if (model.Allocations.Count == 0)
        {
            model.Allocations.Add(new ExpenseAllocationInputViewModel());
        }

        return View(model);
    }




    // POST: EXPENSES/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, EditExpenseViewModel model)
    {
        if (id != model.Id)
            return NotFound();

        var expense = await _context.Expenses
            .Include(e => e.Allocations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
            return NotFound();

        if (model.Allocations == null || model.Allocations.Count == 0)
        {
            ModelState.AddModelError(
                nameof(model.Allocations),
                "La spesa deve contenere almeno una ripartizione.");
        }
        else
        {
            var validCategoryIds = (await _context.Categories
                .Select(c => c.Id)
                .ToListAsync()).ToHashSet();

            var selectedCategories = new HashSet<int>();
            var submittedIds = new HashSet<int>();
            var existingIds = expense.Allocations
                .Select(a => a.Id)
                .ToHashSet();

            for (int i = 0; i < model.Allocations.Count; i++)
            {
                var allocation = model.Allocations[i];

                if (allocation.Amount <= 0)
                {
                    ModelState.AddModelError(
                        $"Allocations[{i}].Amount",
                        "L'importo deve essere maggiore di zero.");
                }

                if (!validCategoryIds.Contains(allocation.CategoryId))
                {
                    ModelState.AddModelError(
                        $"Allocations[{i}].CategoryId",
                        "Seleziona una categoria valida.");
                }
                else if (!selectedCategories.Add(allocation.CategoryId))
                {
                    ModelState.AddModelError(
                        $"Allocations[{i}].CategoryId",
                        "Questa categoria è già presente.");
                }

                if (allocation.Id < 0 ||
                    (allocation.Id > 0 &&
                     (!existingIds.Contains(allocation.Id) ||
                      !submittedIds.Add(allocation.Id))))
                {
                    ModelState.AddModelError(
                        $"Allocations[{i}].Id",
                        "La ripartizione non è valida.");
                }
            }
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync();
            return View(model);
        }

        // Aggiorniamo i dati generali della spesa.
        expense.Description = model.Description;
        expense.Merchant = model.Merchant;
        expense.OccurredOn = model.OccurredOn;

        // Gli ID presenti nel form sono quelli da conservare.
        var retainedIds = model.Allocations
            .Where(a => a.Id > 0)
            .Select(a => a.Id)
            .ToHashSet();

        // Eliminiamo le ripartizioni rimosse dal form.
        foreach (var existing in expense.Allocations
            .Where(a => !retainedIds.Contains(a.Id))
            .ToList())
        {
            _context.ExpenseAllocations.Remove(existing);
        }

        // Aggiorniamo le ripartizioni esistenti o ne creiamo di nuove.
        foreach (var allocation in model.Allocations)
        {
            if (allocation.Id > 0)
            {
                var existing = expense.Allocations
                    .Single(a => a.Id == allocation.Id);

                existing.CategoryId = allocation.CategoryId;
                existing.Amount = allocation.Amount;
            }
            else
            {
                expense.Allocations.Add(new ExpenseAllocation
                {
                    CategoryId = allocation.CategoryId,
                    Amount = allocation.Amount
                });
            }
        }

        await _context.SaveChangesAsync();

        return RedirectToAction(nameof(Index));
    }



    // GET: EXPENSES/Delete/5
    public async Task<IActionResult> Delete(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var expense = await _context.Expenses
            .FirstOrDefaultAsync(m => m.Id == id);
        if (expense == null)
        {
            return NotFound();
        }

        return View(expense);
    }

    // POST: EXPENSES/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int? id)
    {
        var expense = await _context.Expenses.FindAsync(id);
        if (expense != null)
        {
            _context.Expenses.Remove(expense);
        }

        await _context.SaveChangesAsync();
        return RedirectToAction(nameof(Index));
    }

    private bool ExpenseExists(int? id)
    {
        return _context.Expenses.Any(e => e.Id == id);
    }
}
