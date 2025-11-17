using WebApplication2.AI_API_INTEGRATION;
using WebApplication2.dto.BudgetPlanDTO;
using WebApplication2.models;
using WebApplication2.repositories;
using WebApplication2.data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Text.Json;


namespace WebApplication2.service
{
    public class BudgetPlanService
    {
        private readonly IBudgetPlanRepository _repo;
        private readonly GeminiService _gemini;
        private readonly bool _useMock;
        private readonly ApplicationDbContext _context;

        public BudgetPlanService(IBudgetPlanRepository repo, GeminiService gemini, ApplicationDbContext context, bool useMock = false)
        {
            _repo = repo;
            _gemini = gemini;
            _context = context;
            _useMock = useMock;
        }

        public async Task<object> GenerateBudgetPlanAsync(Guid userId, BudgetPlanRequestDTO req, List<Commodity> commodities)
        {
            // --- Auto-fill LocalName ---
            foreach (var c in commodities)
            {
                if (string.IsNullOrWhiteSpace(c.LocalName))
                {
                    c.LocalName = c.ProductName switch
                    {
                        var s when s.Contains("Chicken", StringComparison.OrdinalIgnoreCase) => "Manok",
                        var s when s.Contains("Pork", StringComparison.OrdinalIgnoreCase) => "Baboy",
                        var s when s.Contains("Beef", StringComparison.OrdinalIgnoreCase) => "Baka",
                        var s when s.Contains("Tilapia", StringComparison.OrdinalIgnoreCase) => "Tilapia",
                        var s => s
                    };
                }
            }

            // --- Get DietaryTag from string name ---
            DietaryTag dietaryTag = null;
            if (!string.IsNullOrWhiteSpace(req.DietaryTagId))
            {
                string tagName = req.DietaryTagId.Trim().ToLower();
                dietaryTag = await _context.DietaryTags
                    .AsNoTracking()
                    .FirstOrDefaultAsync(dt => dt.Name.ToLower() == tagName);
            }
            List<Commodity> generationList;

            // If may dietary tag → use your DTO filtering + builder
            if (!string.IsNullOrWhiteSpace(req.DietaryTagId))
            {
                var dtoFiltered = await GetCommoditiesByDietaryTagAsync(req.DietaryTagId);

                if (dtoFiltered.Count == 0)
                {
                    generationList = new List<Commodity>();
                }
                else
                {
                    // Build commodities with LATEST PRICE only
                    generationList = await BuildCommodityListForBudgetAsync(dtoFiltered);
                }
            }
            else
            {
                // No tag → use provided list
                generationList = commodities.ToList();
            }

            // fallback if no commodities match dietary tag
            if (!generationList.Any())
            {
                generationList = commodities.ToList();
            }

            var plan = new BudgetPlan
            {
                UserId = userId,
                TotalBudget = req.TotalBudget,
                DietaryTag = dietaryTag?.Name,
                CreatedAt = DateTime.UtcNow,
                Items = new List<BudgetPlanItem>(),
                MealSuggestions = new List<MealSuggestion>()
            };

            if (_useMock)
                ApplyMock(plan, generationList);
            else
                await GenerateUsingGemini(plan, generationList);

            // Cap items to not exceed budget
            decimal currentTotal = 0;
            plan.Items = plan.Items.Where(i =>
            {
                decimal total = i.Quantity * i.UnitPrice;
                if (currentTotal + total <= plan.TotalBudget)
                {
                    currentTotal += total;
                    return true;
                }
                return false;
            }).ToList();

            await _repo.AddBudgetPlanAsync(plan);

            return new
            {
                plan.BudgetPlanId,
                plan.TotalBudget,
                plan.DietaryTag,
                Items = plan.Items.Select(i =>
                {
                    var latestPrice = i.Commodity.Prices.OrderByDescending(p => p.DateReported).First();
                    return new
                    {
                        i.CommodityId,
                        i.Commodity.ProductName,
                        i.Commodity.LocalName,
                        i.Quantity,
                        i.UnitPrice,
                        Unit = latestPrice.unit,
                        TotalPrice = i.Quantity * i.UnitPrice
                    };
                }).ToList(),
                MealSuggestions = plan.MealSuggestions.Select(m => new
                {
                    meal = m.Name,
                    description = m.Description
                }).ToList()
            };
        }

        private async Task GenerateUsingGemini(BudgetPlan plan, List<Commodity> commodities)
        {
            var rnd = new Random();

            var available = commodities
                .Where(c => c.Prices.Any())
                .OrderBy(x => rnd.Next())
                .Select(c =>
                {
                    var latestPrice = c.Prices.OrderByDescending(p => p.DateReported).First();
                    return new
                    {
                        c.CommodityId,
                        c.ProductName,
                        c.LocalName,
                        Unit = latestPrice.unit,
                        Price = latestPrice.Price
                    };
                })
                .ToList();

            string jsonCommodities = JsonSerializer.Serialize(available);

            string prompt = $@"
You are a Filipino meal planning AI. Output MUST be valid JSON only.

Budget: {plan.TotalBudget}
Members: {plan.Members}

Use ONLY the following products (include LocalName and Unit in output): {jsonCommodities}.
- Generate 5-10 items maximum.
- Total cost must NOT exceed the budget.
- Meals must use ONLY the items generated.
- Limit quantity per item to a maximum of 2 kg.
- Always include Rice, but do not exceed 2 kg.
- Balance the rest of the ingredients across at least 3–5 dishes.
- If there is leftover budget, add additional items to maximize variety.

For each meal:
- Provide 'meal': the dish name.
- Provide 'description': step-by-step cooking instructions in Tagalog.
- Only use ingredients from the generated items.            


Output format:
{{
  ""items"": [
    {{ ""name"": ""string"", ""quantity"": number, ""unitPrice"": number, ""unit"": ""string"" }}
  ],
  ""meals"": [
    {{ ""meal"": ""string"", ""description"": ""string"" }}
  ]
}}

Strict JSON only. No extra words.
";

            string raw;
            try
            {
                raw = await _gemini.GenerateTextAsync(prompt);
            }
            catch
            {
                ApplyMock(plan, commodities);
                return;
            }

            if (string.IsNullOrWhiteSpace(raw))
            {
                ApplyMock(plan, commodities);
                return;
            }

            ParseJson(plan, commodities, raw);

         

        }

        private void ParseJson(BudgetPlan plan, List<Commodity> commodities, string raw)
        {
            try
            {
                raw = raw.Trim();
                if (raw.StartsWith("```json")) raw = raw.Substring(7).Trim();
                if (raw.StartsWith("```")) raw = raw.Substring(3).Trim();
                if (raw.EndsWith("```")) raw = raw.Substring(0, raw.Length - 3).Trim();

                var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                var parsed = JsonSerializer.Deserialize<GeminiBudgetResponse>(raw, options);

                if (parsed?.items != null)
                {
                    foreach (var item in parsed.items)
                    {
                        var c = commodities.FirstOrDefault(x =>
                            item.name.Contains(x.ProductName, StringComparison.OrdinalIgnoreCase) ||
                            x.ProductName.Contains(item.name, StringComparison.OrdinalIgnoreCase));

                        if (c == null || !c.Prices.Any()) continue;

                        plan.Items.Add(new BudgetPlanItem
                        {
                            CommodityId = c.CommodityId,
                            Quantity = item.quantity,
                            UnitPrice = item.unitPrice
                        });

                    }
                }

                if (parsed?.meals != null)
                {
                    foreach (var m in parsed.meals)
                    {
                        if (!string.IsNullOrWhiteSpace(m.meal))
                        {
                            plan.MealSuggestions.Add(new MealSuggestion
                            {
                                Name = m.meal,
                                Description = m.description ?? "",
                                BudgetPlan = plan
                            });
                        }
                    }
                }
            }
            catch
            {
                ApplyMock(plan, commodities);
            }
        }

        private void ApplyMock(BudgetPlan plan, List<Commodity> commodities)
        {
            var rnd = new Random();
            var sample = commodities.Where(c => c.Prices.Any()).OrderBy(x => rnd.Next()).Take(5).ToList();

            foreach (var c in sample)
            {
                var latestPrice = c.Prices.OrderByDescending(p => p.DateReported).First();
                plan.Items.Add(new BudgetPlanItem
                {
                    CommodityId = c.CommodityId,
                    Commodity = c,
                    Quantity = 1,
                    UnitPrice = latestPrice.Price
                });
            }

            plan.MealSuggestions.Add(new MealSuggestion
            {
                Name = "Sample Meal",
                Description = "Sample ingredients from selected commodities",
                BudgetPlan = plan
            });
        }





        private async Task<List<MealSuggestion>> GetMealsFromCategoryRecipesAsync(List<Commodity> commodities)
        {
            var recipeCommodities = commodities
                .Where(c => !string.IsNullOrWhiteSpace(c.Category) && c.Category.Equals("Recipe", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var mealSuggestions = new List<MealSuggestion>();

            foreach (var c in recipeCommodities)
            {
                mealSuggestions.Add(new MealSuggestion
                {
                    Name = c.LocalName ?? c.ProductName,
                    Description = $"Gamitin ang {c.ProductName} sa pagluluto ng masarap na ulam.",
                });
            }

            return mealSuggestions;
        }




        public async Task<List<CommodityDTO>> GetCommoditiesByDietaryTagAsync(string dietaryTagName)
        {
            if (string.IsNullOrWhiteSpace(dietaryTagName))
                return new List<CommodityDTO>();

            string tagName = dietaryTagName.Trim().ToLower();

            var filteredCommodities = await _context.Commodities
                .Include(c => c.ProductDietaryTags)
                    .ThenInclude(dt => dt.DietaryTag)
                .Where(c => c.ProductDietaryTags
                    .Any(dt => dt.DietaryTag.Name.ToLower() == tagName))
                .ToListAsync();

            var dtoList = filteredCommodities.Select(c => new CommodityDTO
            {
                CommodityId = c.CommodityId,
                ProductName = c.ProductName,
                LocalName = c.LocalName,
                Category = c.Category
            }).ToList();

            return dtoList;
        }



        public async Task<List<Commodity>> BuildCommodityListForBudgetAsync(List<CommodityDTO> dtoList)
        {
            var ids = dtoList.Select(d => d.CommodityId).ToList();

            return await _context.Commodities
                .Include(c => c.Prices)
                .Where(c => ids.Contains(c.CommodityId))
                .Select(c => new Commodity
                {
                    CommodityId = c.CommodityId,
                    ProductName = c.ProductName,
                    LocalName = c.LocalName,
                    Category = c.Category,
                    Specification = c.Specification,
                    IsActive = c.IsActive,
                    Prices = c.Prices
                        .OrderByDescending(p => p.DateReported)
                        .Take(1)
                        .ToList()
                })
                .ToListAsync();
        }





    }

    public class GeminiBudgetResponse
    {
        public List<GeminiItem> items { get; set; }
        public List<GeminiMeal> meals { get; set; }
    }

    public class GeminiItem
    {
        public string name { get; set; }
        public decimal quantity { get; set; }
        public decimal unitPrice { get; set; }
        public string unit { get; set; }
    }

    public class GeminiMeal
    {
        public string meal { get; set; }
        public string description { get; set; }
    }
}
