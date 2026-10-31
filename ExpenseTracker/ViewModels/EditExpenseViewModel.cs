
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseTracker.ViewModels
{
    public class EditExpenseViewModel
    {
        public int Id { get; set; }

        public string Description { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public DateOnly OccurredOn { get; set; }

        public int CategoryId { get; set; }
        public decimal Amount { get; set; }

        public List<SelectListItem> Categories { get; set; } = new();
    }
}
