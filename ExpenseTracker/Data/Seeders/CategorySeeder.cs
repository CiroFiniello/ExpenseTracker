
using ExpenseTracker.Models;
using Microsoft.EntityFrameworkCore;

namespace ExpenseTracker.Data.Seeders
{
    public static class CategorySeeder
    {
        public static void Seed(DbContext context)
        {
            string[] categoryNames =
            {
                "Alimentari",
                "Casa",
                "Trasporti",
                "Ristorazione",
                "Abbonamenti",
                "Salute",
                "Altro"
            };

            foreach (var name in categoryNames)
            {
                if (!context.Set<Category>().Any(c => c.Name == name))
                {
                    var category = new Category { Name = name };
                    context.Set<Category>().Add(category);
                }
            }

            context.SaveChanges();
        }
    }
}
