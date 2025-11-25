using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models
{
    public class BudgetPlan {


        [Key]
        public int BudgetPlanId { get; set; }

        public Guid UserId { get; set; }
        public User User { get; set; }   

        public decimal TotalBudget { get; set; }
        public int Days { get; set; }
        public int Members { get; set; }

        public string? DietaryTag { get; set; }   

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public ICollection<BudgetPlanItem> Items { get; set; } = new List<BudgetPlanItem>();
        public ICollection<MealSuggestion> MealSuggestions { get; set; } = new List<MealSuggestion>();
    }
}