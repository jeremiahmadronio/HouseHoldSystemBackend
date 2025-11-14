using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models
{
    public class MealSuggestion {

        [Key]
        public int MealSuggestionId { get; set; }

        public int BudgetPlanId { get; set; }
        public BudgetPlan BudgetPlan { get; set; }

        public string Name { get; set; } = string.Empty;  
        public string? Description { get; set; }

    }
}