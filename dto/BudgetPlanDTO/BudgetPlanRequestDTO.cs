namespace WebApplication2.dto.BudgetPlanDTO
{
	public class BudgetPlanRequestDTO
	{
		public decimal TotalBudget { get; set; }
		
		
		public string? DietaryTagId { get; set; } // optional
	}
}
