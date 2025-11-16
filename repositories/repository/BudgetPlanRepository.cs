using WebApplication2.models;
using WebApplication2.data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebApplication2.repositories.repository
{
    public class BudgetPlanRepository : IBudgetPlanRepository
    {
        private readonly ApplicationDbContext _context;

        public BudgetPlanRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BudgetPlan> AddBudgetPlanAsync(BudgetPlan plan)
        {
            _context.BudgetPlans.Add(plan);
            await _context.SaveChangesAsync();
            return plan;
        }

        public async Task<BudgetPlan?> GetBudgetPlanByIdAsync(int planId)
        {
            return await _context.BudgetPlans
                                 .Include(p => p.Items)
                                 .Include(p => p.MealSuggestions)
                                 .FirstOrDefaultAsync(p => p.BudgetPlanId == planId);
        }

        public async Task<List<BudgetPlan>> GetBudgetPlansByUserAsync(Guid userId)
        {
            return await _context.BudgetPlans
                                 .Include(p => p.Items)
                                 .Include(p => p.MealSuggestions)
                                 .Where(p => p.UserId == userId)
                                 .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }



        public async Task<List<Commodity>> GetCommoditiesByDietaryTagAsync(string dietaryTagName)
        {
            return await _context.Commodities
                .Include(c => c.Prices)
                .Include(c => c.ProductDietaryTags)
                    .ThenInclude(dt => dt.DietaryTag)
                .Where(c => c.ProductDietaryTags
                    .Any(dt => dt.DietaryTag.Name.ToLower() == dietaryTagName.ToLower()))
                .ToListAsync();
        }
    }
}
