using WebApplication2.models;

namespace WebApplication2.repositories;

public interface IBudgetPlanRepository {

    Task<BudgetPlan> AddBudgetPlanAsync(BudgetPlan plan);
    Task<BudgetPlan?> GetBudgetPlanByIdAsync(int planId);
    Task<List<BudgetPlan>> GetBudgetPlansByUserAsync(Guid userId);
    Task SaveChangesAsync();

    Task<List<Commodity>> GetCommoditiesByDietaryTagAsync(string dietaryTagName);
}