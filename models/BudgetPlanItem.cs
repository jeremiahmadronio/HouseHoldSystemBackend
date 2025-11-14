using System.ComponentModel.DataAnnotations;

namespace WebApplication2.models {

	public class BudgetPlanItem {

		[Key]
		public int BudgetPlanItemId { get; set; }

		public int BudgetPlanId { get; set; }
		public BudgetPlan BudgetPlan { get; set; }

		public int CommodityId { get; set; }
		public Commodity Commodity { get; set; }

		public decimal Quantity { get; set; }      
		public decimal UnitPrice { get; set; }

	}
}