
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

    // GET: EXPENSES/Create
    public async Task<IActionResult> Create()
    {
        var model = new CreateExpenseViewModel
        {
            OccurredOn = DateOnly.FromDateTime(DateTime.Today),

            Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync()
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
        // Validazione dell'importo
        if (model.Amount <= 0)
        {
            ModelState.AddModelError(
                nameof(model.Amount),
                "L'importo deve essere maggiore di zero.");
        }

        // Verifica che la categoria esista
        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Seleziona una categoria valida.");
        }

        // Se ci sono errori, torniamo al form
        if (!ModelState.IsValid)
        {
            model.Categories = await _context.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new SelectListItem
                {
                    Value = c.Id.ToString(),
                    Text = c.Name
                })
                .ToListAsync();

            return View(model);
        }

        // Creiamo la spesa
        var expense = new Expense
        {
            Description = model.Description,
            Merchant = model.Merchant,
            OccurredOn = model.OccurredOn
        };

        // Aggiungiamo la ripartizione economica
        expense.Allocations.Add(new ExpenseAllocation
        {
            CategoryId = model.CategoryId,
            Amount = model.Amount
        });

        // Salviamo tutto
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

        if (expense.Allocations.Count > 1)
            return Conflict("Questa spesa contiene più ripartizioni, non ancora supportate dall'editor.");

        var allocation = expense.Allocations.FirstOrDefault();

        var model = new EditExpenseViewModel
        {
            Id = expense.Id,
            Description = expense.Description,
            Merchant = expense.Merchant,
            OccurredOn = expense.OccurredOn,

            CategoryId = allocation?.CategoryId ?? 0,
            Amount = allocation?.Amount ?? 0m,

            Categories = await GetCategoryOptionsAsync()
        };

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

        if (model.Amount <= 0)
        {
            ModelState.AddModelError(
                nameof(model.Amount),
                "L'importo deve essere maggiore di zero.");
        }

        bool categoryExists = await _context.Categories
            .AnyAsync(c => c.Id == model.CategoryId);

        if (!categoryExists)
        {
            ModelState.AddModelError(
                nameof(model.CategoryId),
                "Seleziona una categoria valida.");
        }

        if (!ModelState.IsValid)
        {
            model.Categories = await GetCategoryOptionsAsync();
            return View(model);
        }

        var expense = await _context.Expenses
            .Include(e => e.Allocations)
            .FirstOrDefaultAsync(e => e.Id == id);

        if (expense == null)
            return NotFound();

        if (expense.Allocations.Count > 1)
            return Conflict("Questa spesa contiene più ripartizioni, non ancora supportate dall'editor.");

        expense.Description = model.Description;
        expense.Merchant = model.Merchant;
        expense.OccurredOn = model.OccurredOn;

        var allocation = expense.Allocations.FirstOrDefault();

        if (allocation == null)
        {
            expense.Allocations.Add(new ExpenseAllocation
            {
                CategoryId = model.CategoryId,
                Amount = model.Amount
            });
        }
        else
        {
            allocation.CategoryId = model.CategoryId;
            allocation.Amount = model.Amount;
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
