using System.ComponentModel.DataAnnotations;

namespace ExpenseTracker.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        public ICollection<ExpenseAllocation> Allocations { get; set; } = new List<ExpenseAllocation>();
    }
}
