using WebApplication2.AI_API_INTEGRATION;
using WebApplication2.dto.BudgetPlanDTO;
using WebApplication2.models;
using WebApplication2.repositories;
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

        public BudgetPlanService(IBudgetPlanRepository repo, GeminiService gemini, bool useMock = false)
        {
            _repo = repo;
            _gemini = gemini;
            _useMock = useMock;
        }

        public async Task<object> GenerateBudgetPlanAsync(Guid userId, BudgetPlanRequestDTO req, List<Commodity> commodities)
        {
            // Auto-fill LocalName
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

            // Filter commodities by dietary tag if provided
            if (req.DietaryTagId.HasValue)
            {
                foreach (var c in commodities)
                {
                    c.Prices = c.Prices
                        .Where(p => p.ProductDietaryTags.Any(d => d.DietaryTagId == req.DietaryTagId.Value))
                        .ToList();
                }
            }

            var plan = new BudgetPlan
            {
                UserId = userId,
                TotalBudget = req.TotalBudget,
                DietaryTag = req.DietaryTagId.HasValue ? "Low-Carb" : null,
                CreatedAt = DateTime.UtcNow,
                Items = new List<BudgetPlanItem>(),
                MealSuggestions = new List<MealSuggestion>()
            };

            if (_useMock)
                ApplyMock(plan, commodities);
            else
                await GenerateUsingGemini(plan, commodities);

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
                Items = plan.Items.Select(i => new
                {
                    i.CommodityId,
                    i.Commodity.ProductName,
                    i.Commodity.LocalName,
                    i.Quantity,
                    i.UnitPrice,
                    TotalPrice = i.Quantity * i.UnitPrice
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
            var available = commodities
                .Where(c => c.Prices.Any())
                .Select(c => new
                {
                    c.CommodityId,
                    c.ProductName,
                    c.LocalName,
                    c.Category,
                    Price = c.Prices.OrderByDescending(p => p.DateReported).First().Price
                })
                .ToList();

            string jsonCommodities = JsonSerializer.Serialize(available);

            string prompt = $@"
You are a Filipino meal planning AI. Output MUST be valid JSON only.

Budget: {plan.TotalBudget}
Members: {plan.Members}

Focus on categories: Fish, Rice, Pork Meat, Beef Meat, Chicken, Vegetables.
Use products ONLY from this list: {jsonCommodities}.
Generate 5-10 items max, with real prices.
Ensure total cost does not exceed budget.
Meals must be Filipino-style, realistic, and only use items generated.

For each meal:
- Provide 'meal': the dish name.
- Provide 'description': a simple step-by-step cooking instruction in Tagalog, like teaching someone to cook it. Example: 'Ihugas at timplahan ang tilapia, pagkatapos ay iprito sa mainit na mantika hanggang sa maging golden brown.'

Output format:

{{
  ""items"": [
    {{ ""name"": ""string"", ""quantity"": number, ""unitPrice"": number }}
  ],
  ""meals"": [
    {{ ""meal"": ""string"", ""description"": ""string"" }}
  ]
}}

Use LocalName for all ingredients.
Strict JSON only, no extra words.
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
                if (raw.StartsWith("```json"))
                    raw = raw.Substring(7).Trim();
                if (raw.StartsWith("```"))
                    raw = raw.Substring(3).Trim();
                if (raw.EndsWith("```"))
                    raw = raw.Substring(0, raw.Length - 3).Trim();

                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                var parsed = JsonSerializer.Deserialize<GeminiBudgetResponse>(raw, options);

                if (parsed?.items != null)
                {
                    foreach (var item in parsed.items)
                    {
                        var c = commodities.FirstOrDefault(x =>
                            item.name.Contains(x.ProductName, StringComparison.OrdinalIgnoreCase) ||
                            x.ProductName.Contains(item.name, StringComparison.OrdinalIgnoreCase));

                        if (c == null) continue;

                        plan.Items.Add(new BudgetPlanItem
                        {
                            CommodityId = c.CommodityId,
                            Commodity = c,
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
            catch (Exception ex)
            {
                Console.WriteLine("Parse error. Raw:\n" + raw);
                Console.WriteLine(ex.Message);
            }
        }

        private void ApplyMock(BudgetPlan plan, List<Commodity> commodities)
        {
            var sample = commodities.Take(5).ToList();

            foreach (var c in sample)
            {
                plan.Items.Add(new BudgetPlanItem
                {
                    CommodityId = c.CommodityId,
                    Commodity = c,
                    Quantity = 1,
                    UnitPrice = c.Prices.First().Price
                });
            }

            plan.MealSuggestions.Add(new MealSuggestion
            {
                Name = "Sample Meal",
                Description = "Sample ingredients from selected commodities",
                BudgetPlan = plan
            });
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
    }

    public class GeminiMeal
    {
        public string meal { get; set; }
        public string description { get; set; }
    }
}
