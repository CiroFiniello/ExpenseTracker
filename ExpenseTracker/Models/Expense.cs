    using System.ComponentModel.DataAnnotations;
namespace ExpenseTracker.Models

{
    public class Expense
    {
        [Key]
        public int Id { get; set; }
        public string Description { get; set; } = string.Empty;
        public string Merchant { get; set; } = string.Empty;
        public DateOnly OccurredOn { get; set; }

        public ICollection<ExpenseAllocation> Allocations { get; set; }
    = new List<ExpenseAllocation>();
    }
}
