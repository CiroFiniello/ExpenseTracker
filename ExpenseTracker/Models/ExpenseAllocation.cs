using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class ExpenseAllocation
    {
        [Key]
        public int Id { get; set; }
        public int ExpenseId { get; set; }
        public int CategoryId { get; set; }
        public decimal Amount { get; set; } = 0;

        public Expense Expense { get; set; } = null!;
        public Category Category { get; set; } = null!;
    }
}
